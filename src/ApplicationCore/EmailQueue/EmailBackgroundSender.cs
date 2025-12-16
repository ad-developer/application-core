using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.EmailQueue;

public class EmailBackgroundSender : BackgroundService
{
    private readonly IServiceProvider _services; // resolve scope per run
    private readonly EmailQueueOptions _options;
    private readonly ILogger<EmailBackgroundSender> _logger;
    private readonly string _workerId = Guid.NewGuid().ToString();

    public EmailBackgroundSender(IServiceProvider services, EmailQueueOptions options, ILogger<EmailBackgroundSender> logger)
    {
        _services = services;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailBackgroundSender starting. WorkerId={workerId}", _workerId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in EmailBackgroundSender loop");
                // swallow and continue after delay
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("EmailBackgroundSender stopping.");
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmailQueueDbContext>();
        var smtpFactory = scope.ServiceProvider.GetRequiredService<ISmtpClientFactory>();

        // 1) Select candidates: pending or failed with NextAttemptAt <= now and not locked
        var now = DateTime.UtcNow;
        var candidates = await db.EmailMessages
            .Where(e => (e.Status == EmailStatus.Pending || e.Status == EmailStatus.Failed)
                        && (e.NextAttemptAt == null || e.NextAttemptAt <= now)
                        && (e.LockedUntil == null || e.LockedUntil <= now))
            .OrderBy(e => e.CreatedAt)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (!candidates.Any()) return;

        // 2) Claim them (set LockedBy/LockedUntil) in a safe way
        var lockUntil = DateTime.UtcNow.AddSeconds(_options.LockSeconds);
        foreach (var msg in candidates)
        {
            msg.LockedBy = _workerId;
            msg.LockedUntil = lockUntil;
            msg.Status = EmailStatus.Sending;
        }

        await db.SaveChangesAsync(cancellationToken);

        // 3) Send in parallel with limited degree
        var semaphore = new SemaphoreSlim(_options.MaxDegreeOfParallelism);
        var tasks = new List<Task>();

        foreach (var msg in candidates)
        {
            await semaphore.WaitAsync(cancellationToken);
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await SendSingleAsync(msg, db, smtpFactory, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending email {MessageId}", msg.Id);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
    }

    private async Task SendSingleAsync(EmailMessage msg, EmailQueueDbContext db, ISmtpClientFactory smtpFactory, CancellationToken ct)
    {
        // Reload to ensure we have fresh RowVersion
        var message = await db.EmailMessages.FirstOrDefaultAsync(m => m.Id == msg.Id, ct);
        if (message == null) return;

        // If locked by another worker or lock expired unexpectedly -> skip
        if (message.LockedBy != _workerId)
        {
            _logger.LogDebug("Message {Id} locked by {lockedBy}, skipping", message.Id, message.LockedBy);
            return;
        }

        try
        {
            var mime = MimeMessageFactory.CreateMime(_options, message);

            using var client = await smtpFactory.CreateAndConnectAsync();
            await client.SendAsync(mime, ct);
            await client.DisconnectAsync(true, ct);

            // mark sent
            message.Status = EmailStatus.Sent;
            message.SentAt = DateTime.UtcNow;
            message.LastError = null;
            message.AttemptCount++;
            message.LockedBy = null;
            message.LockedUntil = null;

            db.Update(message);
            await db.SaveChangesAsync(ct);

            _logger.LogInformation("Email {Id} sent to {To}", message.Id, message.To);
        }
        catch (Exception sendEx)
        {
            _logger.LogWarning(sendEx, "Failed to send email {Id}", message.Id);

            message.AttemptCount++;
            message.LastError = sendEx.Message;
            message.LockedBy = null;
            message.LockedUntil = null;

            if (message.AttemptCount >= _options.MaxAttempts)
            {
                message.Status = EmailStatus.DeadLetter;
                _logger.LogWarning("Email {Id} moved to dead letter after {attempts} attempts", message.Id, message.AttemptCount);
            }
            else
            {
                message.Status = EmailStatus.Failed;
                // Exponential backoff for next attempt
                var backoffSeconds = Math.Pow(2, message.AttemptCount) * 30; // 30s, 60s, 120s...
                message.NextAttemptAt = DateTime.UtcNow.AddSeconds(backoffSeconds);
                _logger.LogInformation("Email {Id} scheduled for retry at {when}", message.Id, message.NextAttemptAt);
            }

            db.Update(message);
            await db.SaveChangesAsync(ct);
        }
    }
}

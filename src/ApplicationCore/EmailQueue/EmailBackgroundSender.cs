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
    private readonly ISmtpClientFactory _smtpFactory;
    private readonly IDbContextFactory<EmailQueueDbContext> _dbFactory;

    public EmailBackgroundSender(IServiceProvider services, 
          EmailQueueOptions options, 
          IDbContextFactory<EmailQueueDbContext> dbFactory, 
          ISmtpClientFactory smtpFactory,
          ILogger<EmailBackgroundSender> logger)
    {
        _services = services;
        _options = options;
        _dbFactory = dbFactory;
        _smtpFactory = smtpFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailBackgroundSender started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled background sender error");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var now = DateTime.UtcNow;

        var batch = await db.EmailMessages
            .Where(e =>
                (e.Status == EmailStatus.Pending || e.Status == EmailStatus.Failed) &&
                (e.NextAttemptAt == null || e.NextAttemptAt <= now) &&
                (e.LockedUntil == null || e.LockedUntil <= now))
            .OrderBy(e => e.CreatedAt)
            .Take(_options.BatchSize)
            .ToListAsync(ct);

        if (batch.Count == 0)
            return;

        var lockUntil = DateTime.UtcNow.AddSeconds(_options.LockSeconds);

        foreach (var msg in batch)
        {
            msg.Status = EmailStatus.Sending;
            msg.LockedBy = _workerId;
            msg.LockedUntil = lockUntil;
        }

        await db.SaveChangesAsync(ct);

        var semaphore = new SemaphoreSlim(_options.MaxDegreeOfParallelism);

        var tasks = batch.Select(async msg =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                await SendSingleAsync(msg.Id, ct);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task SendSingleAsync(Guid messageId, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var msg = await db.EmailMessages.FirstOrDefaultAsync(x => x.Id == messageId, ct);
        if (msg == null)
            return;

        // Ensure still owned by this worker
        if (msg.LockedBy != _workerId){

             _logger.LogDebug("Message {Id} locked by {lockedBy}, skipping", msg.Id, msg.LockedBy);
            return;
        }

        try
        {
            var mime = MimeMessageFactory.CreateMime(_options, msg);

            using var client = await _smtpFactory.CreateAndConnectAsync();
            await client.SendAsync(mime, ct);
            await client.DisconnectAsync(true, ct);

            msg.Status = EmailStatus.Sent;
            msg.SentAt = DateTime.UtcNow;
            msg.AttemptCount++;
            msg.LastError = null;
            msg.LockedBy = null;
            msg.LockedUntil = null;

            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Email {Id} sent to {To}", msg.Id, msg.To);
        }
        catch (Exception ex)
        {
            msg.AttemptCount++;
            msg.LastError = ex.Message;
            msg.LockedBy = null;
            msg.LockedUntil = null;

            if (msg.AttemptCount >= _options.MaxAttempts)
            {
                msg.Status = EmailStatus.DeadLetter;
                _logger.LogWarning("Email {Id} moved to dead letter after {attempts} attempts", msg.Id, msg.AttemptCount);
            }
            else
            {
                msg.Status = EmailStatus.Failed;
                
                // Exponential backoff for next attempt
                var backoffSeconds = Math.Pow(2, msg.AttemptCount) * 30; // 30s, 60s, 120s...
                msg.NextAttemptAt = DateTime.UtcNow.AddSeconds(backoffSeconds);

                _logger.LogInformation("Email {Id} scheduled for retry at {when}", msg.Id, msg.NextAttemptAt);
            }

            await db.SaveChangesAsync(ct);
            _logger.LogWarning(ex, "Email {Id} failed (attempt {Attempt})", msg.Id, msg.AttemptCount);
        }
    }
}

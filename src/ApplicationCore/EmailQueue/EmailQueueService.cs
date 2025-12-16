using Microsoft.EntityFrameworkCore;

namespace ApplicationCore.EmailQueue;

public class EmailQueueService : IEmailQueueService
{
    private readonly EmailQueueDbContext _db;

    public EmailQueueService(EmailQueueDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> EnqueueAsync(string to, string subject, string body, string externalId = null,
        string cc = null, string bcc = null)
    {
        // Optional check: idempotency by externalId
        if (!string.IsNullOrEmpty(externalId))
        {
            var existing = await _db.EmailMessages.FirstOrDefaultAsync(m => m.ExternalId == externalId);
            if (existing != null)
                return existing.Id;
        }

        var msg = new EmailMessage
        {
            To = to,
            Subject = subject,
            Body = body,
            ExternalId = externalId,
            Cc = cc,
            Bcc = bcc,
            Status = EmailStatus.Pending,
            NextAttemptAt = DateTime.UtcNow
        };

        _db.EmailMessages.Add(msg);
        await _db.SaveChangesAsync();
        return msg.Id;
    }
}


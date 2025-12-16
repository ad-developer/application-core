namespace ApplicationCore.EmailQueue;

public interface IEmailQueueService
{
    Task<Guid> EnqueueAsync(string to, string subject, string body, string externalId = null,
        string cc = null, string bcc = null);
}


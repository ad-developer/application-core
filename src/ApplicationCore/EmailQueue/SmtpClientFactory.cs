

using MailKit.Net.Smtp;

namespace ApplicationCore.EmailQueue;

public class SmtpClientFactory : ISmtpClientFactory
{
    private readonly EmailQueueOptions _options;

    public SmtpClientFactory(EmailQueueOptions options)
    {
        _options = options;
    }

    public async Task<SmtpClient> CreateAndConnectAsync()
    {
        var client = new SmtpClient();

        // IMPORTANT: in production validate server certificate or configure properly
        await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, _options.SmtpUseSsl);
        if (!string.IsNullOrEmpty(_options.SmtpUser))
        {
            await client.AuthenticateAsync(_options.SmtpUser, _options.SmtpPassword);
        }

        return client;
    }
}

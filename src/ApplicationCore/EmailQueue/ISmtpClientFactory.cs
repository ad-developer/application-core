

using MailKit.Net.Smtp;

namespace ApplicationCore.EmailQueue;

public interface ISmtpClientFactory
{
    Task<SmtpClient> CreateAndConnectAsync();
}

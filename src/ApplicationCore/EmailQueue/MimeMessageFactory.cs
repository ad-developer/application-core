using MimeKit;

namespace ApplicationCore.EmailQueue;

public static class MimeMessageFactory
{
    public static MimeMessage CreateMime(EmailQueueOptions options, EmailMessage msg)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(options.FromName, options.FromAddress));
        mime.To.AddRange(InternetAddressList.Parse(msg.To));
        if (!string.IsNullOrEmpty(msg.Cc))
            mime.Cc.AddRange(InternetAddressList.Parse(msg.Cc));
        if (!string.IsNullOrEmpty(msg.Bcc))
            mime.Bcc.AddRange(InternetAddressList.Parse(msg.Bcc));

        mime.Subject = msg.Subject;
        var body = new TextPart("html") { Text = msg.Body };
        mime.Body = body;
        return mime;
    }
}

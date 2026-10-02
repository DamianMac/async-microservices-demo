using Email.Messages;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Email.Sending;

// Sends mail through the SMTP server in Settings:Smtp, which locally is Mailpit (no auth, no TLS).
public class SmtpSender(SmtpSettings settings)
{
    public async Task Send(SendEmailCommand email, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        message.To.Add(new MailboxAddress(email.ToName ?? "", email.To));
        message.Subject = email.Subject;
        message.Body = new BodyBuilder { TextBody = email.TextBody, HtmlBody = email.HtmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(settings.Host, settings.Port, SecureSocketOptions.None, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}

public class SmtpSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "";
}

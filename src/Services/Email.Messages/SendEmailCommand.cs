using Nimbus.MessageContracts;

namespace Email.Messages;

// Sends one email that's already been rendered. Email handles these itself, so anything
// on the bus can send mail without knowing about SMTP.
public class SendEmailCommand : IBusCommand
{
    public string To { get; set; } = "";
    public string? ToName { get; set; }
    public string Subject { get; set; } = "";
    public string TextBody { get; set; } = "";
    public string? HtmlBody { get; set; }
}

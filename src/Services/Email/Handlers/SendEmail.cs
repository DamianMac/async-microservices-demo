using Email.Messages;
using Email.Sending;
using Nimbus.InfrastructureContracts.Handlers;

namespace Email.Handlers;

// Delivers an already-rendered email over SMTP.
public class SendEmail(SmtpSender sender, ILogger<SendEmail> logger) : IHandleCommand<SendEmailCommand>
{
    public async Task Handle(SendEmailCommand command)
    {
        await sender.Send(command);
        logger.LogInformation("Sent email {Subject} to {To}", command.Subject, command.To);
    }
}

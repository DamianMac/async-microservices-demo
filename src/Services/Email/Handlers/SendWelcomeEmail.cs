using Email.Messages;
using Email.Templates;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Handlers;
using Profile.Messages;

namespace Email.Handlers;

// Competing, so one Email instance handles each registration. Renders the welcome template
// and sends it as a SendEmailCommand, which Email handles itself (SendEmail).
public class SendWelcomeEmail(
    EmailTemplates templates,
    IBus bus,
    IConfiguration configuration,
    ILogger<SendWelcomeEmail> logger) : IHandleCompetingEvent<CustomerRegistered>
{
    public async Task Handle(CustomerRegistered busEvent)
    {
        var email = templates.Render("Welcome", new Dictionary<string, string>
        {
            ["FirstName"] = busEvent.FirstName,
            ["LastName"] = busEvent.LastName,
            ["Email"] = busEvent.Email,
            ["ShopUrl"] = configuration.GetSection("Settings").GetValue<string>("ShopUrl")!.TrimEnd('/'),
        });

        await bus.Send(new SendEmailCommand
        {
            To = busEvent.Email,
            ToName = $"{busEvent.FirstName} {busEvent.LastName}",
            Subject = email.Subject,
            TextBody = email.TextBody,
            HtmlBody = email.HtmlBody,
        });

        logger.LogInformation("Queued welcome email for customer {CustomerId} ({Email})", busEvent.CustomerId, busEvent.Email);
    }
}

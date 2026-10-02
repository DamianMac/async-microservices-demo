using System.Globalization;
using Email.Messages;
using Email.Templates;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Handlers;
using Orders.Messages;

namespace Email.Handlers;

// Competing, so one Email instance tells the customer their order has been dispatched.
public class SendDispatchEmail(
    EmailTemplates templates,
    IBus bus,
    IConfiguration configuration,
    ILogger<SendDispatchEmail> logger) : IHandleCompetingEvent<OrderCompleted>
{
    public async Task Handle(OrderCompleted busEvent)
    {
        var a = busEvent.DeliveryAddress;
        var address = string.Join('\n', new[]
        {
            a.RecipientName,
            a.Line1,
            a.Line2,
            $"{a.Suburb} {a.State} {a.Postcode}",
            a.Country,
        }.Where(l => !string.IsNullOrWhiteSpace(l)));

        var email = templates.Render("OrderDispatched", new Dictionary<string, string>
        {
            ["FirstName"] = busEvent.CustomerFirstName,
            ["OrderId"] = busEvent.OrderId.ToString(),
            // Matches the reference the shop shows (orderRef in useCart.ts).
            ["OrderRef"] = busEvent.OrderId.ToString("N")[..8].ToUpperInvariant(),
            ["Items"] = string.Join('\n', busEvent.Items.Select(i => $"{i.Quantity} × {i.Name}")),
            ["Total"] = $"{busEvent.Total.ToString("C", CultureInfo.GetCultureInfo("en-AU"))} {busEvent.Currency}",
            ["Address"] = address,
            ["ShopUrl"] = configuration.GetSection("Settings").GetValue<string>("ShopUrl")!.TrimEnd('/'),
        });

        await bus.Send(new SendEmailCommand
        {
            To = busEvent.CustomerEmail,
            ToName = $"{busEvent.CustomerFirstName} {busEvent.CustomerLastName}".Trim(),
            Subject = email.Subject,
            TextBody = email.TextBody,
            HtmlBody = email.HtmlBody,
        });

        logger.LogInformation("Queued dispatch email for order {OrderId} to {Email}", busEvent.OrderId, busEvent.CustomerEmail);
    }
}

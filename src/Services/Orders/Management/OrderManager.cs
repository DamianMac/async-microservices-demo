using Microsoft.EntityFrameworkCore;
using Nimbus.InfrastructureContracts;
using Orders.Data;
using Orders.Domain;
using Orders.Messages;
using PaymentGateway.Messages;
using Warehouse.Messages;

namespace Orders.Management;

// Runs an order through its life after it's placed. Each step loads the order, applies what
// happened to it, saves, then publishes the event that starts the next step:
//
//   Pending --PaymentSuccessful--> Paid --(OrderConfirmedEvent, warehouse picks)--OrderProcessed--> Completed --(OrderCompleted)
//
// If the publish fails the handler throws and the event is redelivered. The order has already
// moved on by then, so a step that finds the order already in its target state publishes again
// rather than doing nothing, and the next step isn't lost. Subscribers must tolerate the repeat.
public class OrderManager(IDataContext db, IBus bus, ILogger<OrderManager> logger)
{
    public async Task PaymentSucceeded(PaymentSuccessful payment)
    {
        var order = await Load(payment.OrderId);

        if (order.ApplyPayment(payment.TransactionId, payment.ProcessedAt))
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Order {OrderId} paid with {TransactionId}", order.Id, payment.TransactionId);
        }
        else if (order.Status != OrderStatus.Paid)
        {
            logger.LogInformation("Order {OrderId} is already {Status}; ignoring payment {TransactionId}", order.Id, order.Status, payment.TransactionId);
            return;
        }

        await bus.Publish(new OrderConfirmedEvent
        {
            OrderId = order.Id,
            CustomerName = $"{order.CustomerFirstName} {order.CustomerLastName}".Trim(),
            DeliveryAddress = AddressOf(order),
            PickList = ItemsOf(order),
            ConfirmedAt = DateTime.UtcNow,
        });
    }

    public async Task Processed(OrderProcessed processed)
    {
        var order = await Load(processed.OrderId);

        if (order.Complete(processed.DispatchedAt))
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Order {OrderId} completed, dispatched at {DispatchedAt}", order.Id, processed.DispatchedAt);
        }
        else if (order.Status != OrderStatus.Completed)
        {
            logger.LogWarning("Order {OrderId} is {Status}, not Paid; ignoring its dispatch", order.Id, order.Status);
            return;
        }

        await bus.Publish(new OrderCompleted
        {
            OrderId = order.Id,
            CustomerEmail = order.CustomerEmail,
            CustomerFirstName = order.CustomerFirstName,
            CustomerLastName = order.CustomerLastName,
            DeliveryAddress = AddressOf(order),
            Items = ItemsOf(order),
            Total = order.Total,
            Currency = order.Currency,
            CompletedAt = order.CompletedAt!.Value,
        });
    }

    private async Task<Order> Load(Guid orderId) =>
        await db.Orders.Include(o => o.Lines).SingleOrDefaultAsync(o => o.Id == orderId)
        ?? throw new InvalidOperationException($"Order {orderId} doesn't exist.");

    private static DeliveryAddress AddressOf(Order order) => new()
    {
        RecipientName = order.DeliveryAddress.RecipientName,
        Line1 = order.DeliveryAddress.Line1,
        Line2 = order.DeliveryAddress.Line2,
        Suburb = order.DeliveryAddress.Suburb,
        State = order.DeliveryAddress.State,
        Postcode = order.DeliveryAddress.Postcode,
        Country = order.DeliveryAddress.Country,
        Phone = order.DeliveryAddress.Phone,
    };

    private static List<PickListItem> ItemsOf(Order order) => order.Lines
        .OrderBy(l => l.Id)
        .Select(l => new PickListItem { Sku = l.Sku, Name = l.Name, Quantity = l.Quantity })
        .ToList();
}

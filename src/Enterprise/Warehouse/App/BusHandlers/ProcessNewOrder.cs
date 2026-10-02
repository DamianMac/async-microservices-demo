using Marten;
using Nimbus.InfrastructureContracts.Handlers;
using Orders.Messages;
using Warehouse.Domain;

namespace Warehouse.App.BusHandlers;

// Competing, so one Warehouse instance queues each confirmed order as a pending dispatch.
// Keyed by order id, so a redelivered event doesn't queue it twice or reopen a dispatched one.
public class ProcessNewOrder(IDocumentSession session, ILogger<ProcessNewOrder> logger) : IHandleCompetingEvent<OrderConfirmedEvent>
{
    public async Task Handle(OrderConfirmedEvent busEvent)
    {
        if (await session.LoadAsync<Dispatch>(busEvent.OrderId) is { } existing)
        {
            logger.LogInformation("Order {OrderId} is already queued ({Status}); ignoring", busEvent.OrderId, existing.Status);
            return;
        }

        var address = busEvent.DeliveryAddress;
        var dispatch = new Dispatch
        {
            OrderId = busEvent.OrderId,
            CustomerName = busEvent.CustomerName,
            DeliveryAddress = new DispatchAddress
            {
                RecipientName = address.RecipientName,
                Line1 = address.Line1,
                Line2 = address.Line2,
                Suburb = address.Suburb,
                State = address.State,
                Postcode = address.Postcode,
                Country = address.Country,
                Phone = address.Phone,
            },
            PickList = busEvent.PickList
                .Select(i => new PickLine { Sku = i.Sku, Name = i.Name, Quantity = i.Quantity })
                .ToList(),
            Status = DispatchStatus.Pending,
            ReceivedAt = DateTime.UtcNow,
        };

        session.Insert(dispatch);
        await session.SaveChangesAsync();

        logger.LogInformation("Queued order {OrderId} for dispatch: {Units} units across {Lines} SKUs",
            dispatch.OrderId, dispatch.Units, dispatch.PickList.Count);
    }
}

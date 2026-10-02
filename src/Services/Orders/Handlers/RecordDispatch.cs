using Nimbus.InfrastructureContracts.Handlers;
using Orders.Management;
using Warehouse.Messages;

namespace Orders.Handlers;

// Competing, so one Orders instance completes each dispatched order.
public class RecordDispatch(OrderManager orderManager) : IHandleCompetingEvent<OrderProcessed>
{
    public Task Handle(OrderProcessed busEvent) => orderManager.Processed(busEvent);
}

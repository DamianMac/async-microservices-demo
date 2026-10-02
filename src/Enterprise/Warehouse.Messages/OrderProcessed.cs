using Nimbus.MessageContracts;

namespace Warehouse.Messages;

// Published by the warehouse when an order has been picked, packed and dispatched.
public class OrderProcessed : IBusEvent
{
    public Guid OrderId { get; set; }
    public DateTime DispatchedAt { get; set; }
}

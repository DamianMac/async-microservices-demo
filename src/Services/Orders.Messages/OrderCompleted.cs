using Nimbus.MessageContracts;

namespace Orders.Messages;

// Published by Orders when the warehouse has dispatched an order. It carries the customer's
// email and name, so Email can tell them without calling back to Orders or Profile.
public class OrderCompleted : IBusEvent
{
    public Guid OrderId { get; set; }
    public string CustomerEmail { get; set; } = "";
    public string CustomerFirstName { get; set; } = "";
    public string CustomerLastName { get; set; } = "";
    public DeliveryAddress DeliveryAddress { get; set; } = new();
    public List<PickListItem> Items { get; set; } = [];
    public decimal Total { get; set; }
    public string Currency { get; set; } = "";
    public DateTime CompletedAt { get; set; }
}

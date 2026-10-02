using Nimbus.MessageContracts;

namespace Orders.Messages;

// Published by Orders once an order is paid, so the warehouse can pick and dispatch it.
// It carries everything the warehouse needs, so it doesn't call back to Orders.
public class OrderConfirmedEvent : IBusEvent
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public DeliveryAddress DeliveryAddress { get; set; } = new();
    public List<PickListItem> PickList { get; set; } = [];
    public DateTime ConfirmedAt { get; set; }
}

public class PickListItem
{
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
}

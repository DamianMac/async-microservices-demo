using Nimbus.MessageContracts;

namespace Orders.Messages;

// Published by Orders when a customer places an order. It carries the amount and the card
// as the customer chose it at checkout, so PaymentGateway can take the payment without
// calling back to Orders.
public class ChargeCustomerCommand : IBusCommand
{
    public Guid OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "";
    public int PaymentCardId { get; set; }
    public string CardBrand { get; set; } = "";
    public string CardLast4 { get; set; } = "";
    public DateTime PlacedAt { get; set; }
}

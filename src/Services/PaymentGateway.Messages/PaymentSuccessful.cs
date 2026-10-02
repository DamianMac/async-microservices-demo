using Nimbus.MessageContracts;

namespace PaymentGateway.Messages;

// Published by PaymentGateway when an order's card payment goes through.
public class PaymentSuccessful : IBusEvent
{
    public Guid OrderId { get; set; }
    public string TransactionId { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "";
    public DateTime ProcessedAt { get; set; }
}

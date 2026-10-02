using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Handlers;
using Orders.Messages;
using PaymentGateway.Messages;
using PaymentGateway.Payments;

namespace PaymentGateway.Handlers;

// Competing, so one PaymentGateway instance charges each order. The processor fails the
// first attempt, so this throws and Nimbus redelivers the event with a backoff.
public class TakePayment(
    MockCardProcessor processor,
    IBus bus,
    ILogger<TakePayment> logger) : IHandleCommand<ChargeCustomerCommand>
{
    public async Task Handle(ChargeCustomerCommand busEvent)
    {
        var transactionId = await processor.Charge(
            busEvent.OrderId, busEvent.Amount, busEvent.Currency, busEvent.CardBrand, busEvent.CardLast4);

        logger.LogInformation("Payment {TransactionId} of {Amount} {Currency} succeeded for order {OrderId}",
            transactionId, busEvent.Amount, busEvent.Currency, busEvent.OrderId);

        await bus.Publish(new PaymentSuccessful
        {
            OrderId = busEvent.OrderId,
            TransactionId = transactionId,
            Amount = busEvent.Amount,
            Currency = busEvent.Currency,
            ProcessedAt = DateTime.UtcNow,
        });
    }
}

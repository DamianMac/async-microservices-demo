using Nimbus.InfrastructureContracts.Handlers;
using Orders.Management;
using PaymentGateway.Messages;

namespace Orders.Handlers;

// Competing, so one Orders instance marks each order paid.
public class RecordPayment(OrderManager orderManager) : IHandleCompetingEvent<PaymentSuccessful>
{
    public Task Handle(PaymentSuccessful busEvent) => orderManager.PaymentSucceeded(busEvent);
}

using Autofac;
using PaymentGateway.Payments;

namespace PaymentGateway.Infrastructure;

public class PaymentsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Single instance, so its record of attempts survives between deliveries.
        builder.RegisterType<MockCardProcessor>().SingleInstance();
    }
}

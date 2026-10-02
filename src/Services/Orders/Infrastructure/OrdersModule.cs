using Autofac;
using Orders.Management;

namespace Orders.Infrastructure;

public class OrdersModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<OrderManager>().InstancePerLifetimeScope();
    }
}

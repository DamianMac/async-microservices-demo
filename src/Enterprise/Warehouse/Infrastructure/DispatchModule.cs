using Autofac;
using Warehouse.Dispatching;
using Warehouse.Domain;

namespace Warehouse.Infrastructure;

public class DispatchModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<DispatchCompleter>().InstancePerLifetimeScope();
        builder.RegisterType<StockAlerts>().InstancePerLifetimeScope();
    }
}

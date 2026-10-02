using Autofac;
using Warehouse.Import;

namespace Warehouse.Infrastructure;

public class ImportModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<StockImporter>().AsSelf().InstancePerLifetimeScope();
    }
}

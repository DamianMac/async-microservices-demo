using Autofac;
using CatalogManager.Import;

namespace CatalogManager.Infrastructure;

public class ImportModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ProductImporter>().AsSelf().InstancePerLifetimeScope();
    }
}

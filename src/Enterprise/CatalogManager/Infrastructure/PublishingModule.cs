using Autofac;
using CatalogManager.Publishing;

namespace CatalogManager.Infrastructure;

public class PublishingModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CatalogPublisher>().AsSelf().InstancePerLifetimeScope();
    }
}

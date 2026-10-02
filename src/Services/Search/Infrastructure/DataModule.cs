using Autofac;
using Microsoft.EntityFrameworkCore;
using Search.Data;

namespace Search.Infrastructure;

public class DataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(ctx =>
        {
            var connectionString = ctx.Resolve<IConfiguration>().GetSection("Settings").GetValue<string>("ConnectionString")!;
            return new DbContextOptionsBuilder<DataContext>().UseNpgsql(connectionString).Options;
        }).SingleInstance();

        builder.RegisterType<DataContext>().AsSelf().As<IDataContext>().InstancePerLifetimeScope();
    }
}

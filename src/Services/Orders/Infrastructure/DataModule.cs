using Autofac;
using Microsoft.EntityFrameworkCore;
using Orders.Data;

namespace Orders.Infrastructure;

public class DataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(ctx =>
        {
            var connectionString = ctx.Resolve<IConfiguration>().GetSection("Settings").GetValue<string>("ConnectionString")!;
            return new DbContextOptionsBuilder<DataContext>().UseSqlServer(connectionString).Options;
        }).SingleInstance();

        builder.RegisterType<DataContext>().AsSelf().As<IDataContext>().InstancePerLifetimeScope();
    }
}

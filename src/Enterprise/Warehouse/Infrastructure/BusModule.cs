using Autofac;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.DeliveryRetries;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger.Serilog.Configuration;
using Nimbus.Serializers.Json.Configuration;
using Nimbus.Transports.Nats;
using Orders.Messages;
using Serilog;
using Shared.Messaging;
using Warehouse.Messages;

namespace Warehouse.Infrastructure;

public class BusModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
     
        //You need to reference every assembly that contains messages you want to receive
        //Plus the assembly that contains the message handler
        var typeProvider = new AssemblyScanningTypeProvider(
            typeof(BusModule).Assembly,
            typeof(CorrelationIdInterceptor).Assembly,
            typeof(OrderConfirmedEvent).Assembly,
            typeof(OrderProcessed).Assembly);
        builder.RegisterNimbus(typeProvider);
        builder.Register(ctx =>
        {
            var config = ctx.Resolve<IConfiguration>();
            var settings = config.GetSection("Settings");
            var natsUrl = settings.GetValue<string>("NatsUrl")!;
            var natsUser = settings.GetValue<string>("NatsUser")!;
            var natsPassword = settings.GetValue<string>("NatsPassword")!;
            var instance = config.GetValue<string>("Instance") ?? Environment.MachineName;

            Log.Information("Starting bus with instance {Instance} on NATS {natusUrl}", instance, natsUrl);
            return new BusBuilder()
                .Configure()
                .WithNames("Warehouse", instance)
                .WithTransport(
                    new NatsTransportConfiguration()
                        .WithUrl(natsUrl)
                        .WithCredentials(natsUser, natsPassword)
                        .WithJetStream()
                    )
                .WithTypesFrom(typeProvider)
                .WithAutofacDefaults(ctx)
                .WithJsonSerializer()
                .WithSerilogLogger()
                .WithHeartbeatInterval(TimeSpan.FromMinutes(2))
                .WithDeliveryRetryStrategy(new SimpleBackoffDeliveryRetryStrategy())
                .WithGlobalInboundInterceptorTypes(typeof(CorrelationIdInterceptor))
                .WithGlobalOutboundInterceptorTypes(typeof(OutgoingMessageLoggingInterceptor))
                .Build();
        }).AsSelf().As<IBus>().SingleInstance();
    }
}
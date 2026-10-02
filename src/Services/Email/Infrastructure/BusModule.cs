using Autofac;
using Email.Messages;
using Orders.Messages;
using Profile.Messages;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.DeliveryRetries;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger.Serilog.Configuration;
using Nimbus.Serializers.Json.Configuration;
using Nimbus.Transports.Nats;
using Serilog;
using Shared.Messaging;

namespace Email.Infrastructure;

public class BusModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
     
        
        // The app's own assembly, the shared interceptors, and every assembly with messages it sends or receives.
        var typeProvider = new AssemblyScanningTypeProvider(
            typeof(BusModule).Assembly,
            typeof(CorrelationIdInterceptor).Assembly,
            typeof(SendEmailCommand).Assembly,
            typeof(CustomerRegistered).Assembly,
            typeof(OrderCompleted).Assembly);
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
                .WithNames("Email", instance)
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
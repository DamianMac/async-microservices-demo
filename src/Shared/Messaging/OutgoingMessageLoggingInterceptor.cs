using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Serilog;

namespace Shared.Messaging;

// Logs every message the app sends, with its id and correlation id.
public class OutgoingMessageLoggingInterceptor : OutboundInterceptor, IRequireDispatchContext
{
    public IDispatchContext DispatchContext { get; set; } = null!;

    public override Task OnCommandSending<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
    {
        Log.Information(
            "Sending command {MessageId} with correlation {CorrelationId} of type {MessageType}",
            nimbusMessage.MessageId,
            nimbusMessage.CorrelationId,
            typeof(TBusCommand).Name);
        return base.OnCommandSending(busCommand, nimbusMessage);
    }

    public override Task OnEventPublishing<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage)
    {
        Log.Information(
            "Publishing event {MessageId} with correlation {CorrelationId} of type {MessageType}",
            nimbusMessage.MessageId,
            nimbusMessage.CorrelationId,
            typeof(TBusEvent).Name);
        return base.OnEventPublishing(busEvent, nimbusMessage);
    }

    public override Task OnRequestSending<TBusRequest, TBusResponse>(
        IBusRequest<TBusRequest, TBusResponse> busRequest,
        NimbusMessage nimbusMessage)
    {
        Log.Information(
            "Sending request {MessageId} with correlation {CorrelationId} of type {MessageType}",
            nimbusMessage.MessageId,
            nimbusMessage.CorrelationId,
            typeof(TBusRequest).Name);
        return base.OnRequestSending(busRequest, nimbusMessage);
    }
}

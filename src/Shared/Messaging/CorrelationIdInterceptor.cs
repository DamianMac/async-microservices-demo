using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Interceptors.Inbound;
using Nimbus.MessageContracts;

namespace Shared.Messaging;

// Puts the incoming message's correlation id in the logical call context before a
// handler runs, so CorrelationIdEnricher can tag every log the handler writes.
public class CorrelationIdInterceptor : InboundInterceptor, IRequireDispatchContext
{
    public IDispatchContext DispatchContext { get; set; } = null!;

    public override Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
    {
        SetCallContext(nimbusMessage);
        return base.OnCommandHandlerExecuting(busCommand, nimbusMessage);
    }

    public override Task OnRequestHandlerExecuting<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
    {
        SetCallContext(nimbusMessage);
        return base.OnRequestHandlerExecuting(busRequest, nimbusMessage);
    }

    public override Task OnEventHandlerExecuting<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage)
    {
        SetCallContext(nimbusMessage);
        return base.OnEventHandlerExecuting(busEvent, nimbusMessage);
    }

    private void SetCallContext(NimbusMessage nimbusMessage)
    {
        CallContext.LogicalSetData(CorrelationIdEnricher.CorrelationId, DispatchContext.CorrelationId);
        CallContext.LogicalSetData(CorrelationIdEnricher.ResultOfMessageId, DispatchContext.ResultOfMessageId);
        CallContext.LogicalSetData(CorrelationIdEnricher.MessageId, nimbusMessage.MessageId);
    }
}

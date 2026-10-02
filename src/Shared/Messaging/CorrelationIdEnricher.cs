using Nimbus.Infrastructure;
using Serilog.Core;
using Serilog.Events;

namespace Shared.Messaging;

// Adds the correlation id, message id and result-of message id set by
// CorrelationIdInterceptor to every log written while a handler runs.
public class CorrelationIdEnricher : ILogEventEnricher
{
    public const string CorrelationId = nameof(CorrelationId);
    public const string MessageId = nameof(MessageId);
    public const string ResultOfMessageId = nameof(ResultOfMessageId);

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        AddProperty(logEvent, propertyFactory, CorrelationId);
        AddProperty(logEvent, propertyFactory, MessageId);
        AddProperty(logEvent, propertyFactory, ResultOfMessageId);
    }

    private static void AddProperty(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, string name)
    {
        if (CallContext.LogicalGetData(name) is Guid value)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(name, value));
        }
    }
}

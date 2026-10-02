using Catalog.Domain;
using Marten;
using Nimbus.InfrastructureContracts.Handlers;
using Warehouse.Messages;

namespace Catalog.Handlers;

// Competing, so one Catalog instance stores each warehouse stock alert.
// UpdateCachedStockLevel handles the same events on every instance for the cache.
public class RecordStockLevel(IDocumentSession session, ILogger<RecordStockLevel> logger)
    : IHandleCompetingEvent<LowStockWarning>, IHandleCompetingEvent<OutOfStock>
{
    public Task Handle(LowStockWarning busEvent) => Record(busEvent.Sku, StockLevel.Low);

    public Task Handle(OutOfStock busEvent) => Record(busEvent.Sku, StockLevel.Out);

    private async Task Record(string sku, StockLevel level)
    {
        session.Store(new StockStatus { Sku = sku, Level = level, UpdatedAt = DateTime.UtcNow });
        await session.SaveChangesAsync();
        logger.LogInformation("Recorded {Sku} as {StockLevel}", sku, level);
    }
}

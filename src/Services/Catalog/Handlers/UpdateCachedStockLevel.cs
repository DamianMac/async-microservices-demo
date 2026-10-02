using Catalog.Caching;
using Catalog.Domain;
using Nimbus.InfrastructureContracts.Handlers;
using Warehouse.Messages;

namespace Catalog.Handlers;

// Multicast, so every Catalog instance updates its own cache straight from the event,
// without waiting for RecordStockLevel to store it.
public class UpdateCachedStockLevel(ProductCache cache, ILogger<UpdateCachedStockLevel> logger)
    : IHandleMulticastEvent<LowStockWarning>, IHandleMulticastEvent<OutOfStock>
{
    public Task Handle(LowStockWarning busEvent) => Update(busEvent.Sku, StockLevel.Low);

    public Task Handle(OutOfStock busEvent) => Update(busEvent.Sku, StockLevel.Out);

    private async Task Update(string sku, StockLevel level)
    {
        await cache.SetStockLevel(sku, level);
        logger.LogInformation("Cached {Sku} as {StockLevel}", sku, level);
    }
}

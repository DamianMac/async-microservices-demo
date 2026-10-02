using Catalog.Caching;
using Catalog.Messages;
using Marten;
using Nimbus.InfrastructureContracts.Handlers;

namespace Catalog.Handlers;

// Multicast, so every Catalog instance reloads its own cache.
public class RefreshProductCache(
    ProductCache cache,
    IQuerySession session,
    ILogger<RefreshProductCache> logger) : IHandleMulticastEvent<CatalogUpdated>
{
    public async Task Handle(CatalogUpdated busEvent)
    {
        await cache.Reload(session);
        logger.LogInformation("Reloaded {Count} products into the cache for the catalog published at {PublishDate}",
            cache.Products.Count, busEvent.PublishDate);
    }
}

using Marten;

namespace Catalog.Caching;

// Fills the cache at startup, so an instance serves products before the next CatalogUpdated.
public class LoadProductCache(ProductCache cache, IServiceScopeFactory scopes, ILogger<LoadProductCache> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        await cache.Reload(scope.ServiceProvider.GetRequiredService<IQuerySession>(), cancellationToken);
        logger.LogInformation("Loaded {Count} products into the cache", cache.Products.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

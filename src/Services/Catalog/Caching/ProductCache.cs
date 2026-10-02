using Catalog.Domain;
using Catalog.Images;
using Catalog.Models;
using Marten;

namespace Catalog.Caching;

// Every stored product, held in memory to serve GET /products, with its stock level.
// Loaded at startup and reloaded on each instance when CatalogUpdated arrives. Stock alerts
// update single SKUs in place.
public class ProductCache(ProductImages images)
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IReadOnlyList<ProductResponse>? _products;
    // Every SKU the warehouse has reported, including ones not in the catalog yet.
    private Dictionary<string, StockLevel> _stockLevels = [];

    public IReadOnlyList<ProductResponse> Products => _products ?? [];

    public async Task Reload(IQuerySession session, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var products = await session.Query<Product>()
                .OrderBy(p => p.Sku)
                .ToListAsync(cancellationToken);
            var statuses = await session.Query<StockStatus>().ToListAsync(cancellationToken);

            // Keep levels cached from events whose competing handler hasn't stored them yet.
            var stockLevels = new Dictionary<string, StockLevel>(_stockLevels);
            foreach (var s in statuses)
                stockLevels[s.Sku] = s.Level;

            _stockLevels = stockLevels;
            _products = products.Select(p => ToResponse(p, LevelFor(p.Sku))).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetStockLevel(string sku, StockLevel level)
    {
        await _lock.WaitAsync();
        try
        {
            _stockLevels = new Dictionary<string, StockLevel>(_stockLevels) { [sku] = level };
            _products = Products.Select(p => p.Sku == sku ? p with { StockLevel = level } : p).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    private StockLevel LevelFor(string sku) => _stockLevels.GetValueOrDefault(sku, StockLevel.InStock);

    private ProductResponse ToResponse(Product p, StockLevel stockLevel) => new(
        p.Sku,
        p.Name,
        p.Category,
        p.Description,
        p.Price,
        p.Currency,
        p.ImageFileName is null ? null : images.UrlFor(p.ImageFileName),
        p.Coffee,
        p.UpdatedAt,
        stockLevel);
}

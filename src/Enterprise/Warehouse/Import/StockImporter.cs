using System.Text.Json;
using Marten;
using Warehouse.Domain;

namespace Warehouse.Import;

public record ImportResult(int Added, int Updated);

// Imports the product list JSON (data/products.json) into the warehouse, matching on SKU.
// Only the SKU, name, stock and weight are kept. Every low or empty SKU gets a
// LowStockWarning or OutOfStock, so the shop learns the levels a fresh import starts with.
public class StockImporter(IDocumentSession session, IWebHostEnvironment env, StockAlerts stockAlerts, ILogger<StockImporter> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<ImportResult> ImportAsync(string jsonPath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(jsonPath, env.ContentRootPath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"No product list found at {fullPath}", fullPath);

        await using var stream = File.OpenRead(fullPath);
        var file = await JsonSerializer.DeserializeAsync<ProductFile>(stream, JsonOptions, cancellationToken)
                   ?? throw new InvalidDataException($"{fullPath} is empty");

        var existing = (await session.Query<StockItem>().ToListAsync(cancellationToken))
            .ToDictionary(i => i.Sku);
        var now = DateTime.UtcNow;
        int added = 0, updated = 0;
        var stored = new List<StockItem>();

        foreach (var product in file.Products)
        {
            if (!existing.TryGetValue(product.Sku, out var item))
            {
                // New items take the file's stock count; after that stock is managed here,
                // so a re-import doesn't overwrite it.
                item = new StockItem { Sku = product.Sku, Stock = product.Stock, CreatedAt = now };
                added++;
            }
            else
            {
                updated++;
            }

            item.Name = product.Name;
            item.WeightGrams = product.Coffee?.WeightGrams;
            item.UpdatedAt = now;
            session.Store(item);
            stored.Add(item);
        }

        await session.SaveChangesAsync(cancellationToken);

        foreach (var item in stored)
            await stockAlerts.PublishFor(item);

        logger.LogInformation("Imported {Count} stock items from {Path}: {Added} added, {Updated} updated",
            file.Products.Count, fullPath, added, updated);

        return new ImportResult(added, updated);
    }

    private record ProductFile(List<ProductItem> Products);

    private record ProductItem(string Sku, string Name, int Stock, CoffeeItem? Coffee);

    private record CoffeeItem(int WeightGrams);
}

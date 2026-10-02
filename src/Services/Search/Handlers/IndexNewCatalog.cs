using CatalogManager.Messages;
using Microsoft.EntityFrameworkCore;
using Nimbus.InfrastructureContracts.Handlers;
using Search.CatalogManager;
using Search.Data;
using Search.Domain;

namespace Search.Handlers;

// Replaces the indexed products with the published ones: upserts by SKU and removes products no longer published.
public class IndexNewCatalog(
    CatalogManagerClient catalogManager,
    IDataContext db,
    ILogger<IndexNewCatalog> logger) : IHandleCompetingEvent<NewCatalogAvailable>
{
    public async Task Handle(NewCatalogAvailable busEvent)
    {
        var published = await catalogManager.GetPublishedProducts();
        logger.LogInformation("Indexing {Count} products from the catalog published at {PublishDate}", published.Count, busEvent.PublishDate);

        // Locks the table for the transaction, so handlers running at the same time (e.g. a backlog
        // of publications) take turns rather than racing to insert the same SKUs. Reads still work.
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("LOCK TABLE products IN EXCLUSIVE MODE");

        var existing = await db.Products.ToDictionaryAsync(p => p.Sku);

        foreach (var p in published)
        {
            if (!existing.Remove(p.Sku, out var product))
            {
                product = new Product { Sku = p.Sku };
                db.Products.Add(product);
            }

            product.Name = p.Name;
            product.Category = p.Category;
            product.Description = p.Description;
            product.Origin = p.Coffee?.Origin;
            product.Region = p.Coffee?.Region;
            product.Process = p.Coffee?.Process;
            product.Roast = p.Coffee?.Roast;
            product.TastingNotes = p.Coffee is null ? null : string.Join(", ", p.Coffee.TastingNotes);
        }

        // Whatever's left wasn't in the published catalog.
        db.Products.RemoveRange(existing.Values);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}

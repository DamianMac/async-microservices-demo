using CatalogManager.Messages;
using Microsoft.EntityFrameworkCore;
using Nimbus.InfrastructureContracts.Handlers;
using Orders.CatalogManager;
using Orders.Data;
using Orders.Domain;

namespace Orders.Handlers;

// Replaces Orders' copy of the catalog's names and prices with the published ones: upserts by SKU
// and removes products no longer published, so they can't be ordered.
public class UpdatePriceList(
    CatalogManagerClient catalogManager,
    IDataContext db,
    ILogger<UpdatePriceList> logger) : IHandleCompetingEvent<NewCatalogAvailable>
{
    public async Task Handle(NewCatalogAvailable busEvent)
    {
        var published = await catalogManager.GetPublishedProducts();
        logger.LogInformation("Updating prices for {Count} products from the catalog published at {PublishDate}", published.Count, busEvent.PublishDate);

        // Locks the table for the transaction, so handlers running at the same time (e.g. a backlog
        // of publications) take turns rather than racing to insert the same SKUs.
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("SELECT TOP 0 Sku FROM dbo.Products WITH (TABLOCKX, HOLDLOCK)");

        var existing = await db.Products.ToDictionaryAsync(p => p.Sku);
        var now = DateTime.UtcNow;

        foreach (var p in published)
        {
            if (!existing.Remove(p.Sku, out var product))
            {
                product = new Product { Sku = p.Sku };
                db.Products.Add(product);
            }

            product.Name = p.Name;
            product.Price = p.Price;
            product.Currency = p.Currency;
            product.UpdatedAt = now;
        }

        // Whatever's left wasn't in the published catalog.
        db.Products.RemoveRange(existing.Values);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}

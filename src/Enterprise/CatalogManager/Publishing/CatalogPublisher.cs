using CatalogManager.Data;
using CatalogManager.Domain;
using CatalogManager.Messages;
using Microsoft.EntityFrameworkCore;
using Nimbus.InfrastructureContracts;

namespace CatalogManager.Publishing;

// Publishes every priced product, then announces the new catalog on the bus. Subscribers
// fetch the products themselves from GET /api/products; the event only says when.
public class CatalogPublisher(IDataContext db, IBus bus, ILogger<CatalogPublisher> logger)
{
    public async Task<CatalogPublication> PublishAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var products = await db.Products.Where(p => p.Price != null).ToListAsync(cancellationToken);
        foreach (var product in products)
        {
            product.Status = ProductStatus.Published;
            product.PublishedAt = now;
        }

        var publication = new CatalogPublication { PublishedAt = now, ProductCount = products.Count };
        db.CatalogPublications.Add(publication);

        // Save before announcing, so a subscriber that fetches straight away sees the new catalog.
        // If the publish to the bus fails, publishing again is safe.
        await db.SaveChangesAsync(cancellationToken);
        await bus.Publish(new NewCatalogAvailable { PublishDate = new DateTimeOffset(now) });

        logger.LogInformation("Published catalog of {ProductCount} products and sent {Event}", products.Count, nameof(NewCatalogAvailable));
        return publication;
    }
}

using Catalog.CatalogManager;
using Catalog.Domain;
using Catalog.Images;
using Catalog.Messages;
using CatalogManager.Messages;
using Marten;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Handlers;

namespace Catalog.Handlers;

// Competing, so only one Catalog instance handles each publication.
// Replaces the stored catalog with the published products, downloads their images,
// then publishes CatalogUpdated.
public class StoreNewCatalog(
    CatalogManagerClient catalogManager,
    ProductImages images,
    IDocumentSession session,
    IBus bus,
    ILogger<StoreNewCatalog> logger) : IHandleCompetingEvent<NewCatalogAvailable>
{
    public async Task Handle(NewCatalogAvailable busEvent)
    {
        var published = await catalogManager.GetPublishedProducts();
        logger.LogInformation("Storing {Count} products from the catalog published at {PublishDate}", published.Count, busEvent.PublishDate);

        Directory.CreateDirectory(images.Folder);
        foreach (var p in published.Where(p => p.ImageUrl is not null && p.ImageFileName is not null))
        {
            await catalogManager.DownloadTo(p.ImageUrl!, images.PathFor(p.ImageFileName!));
        }

        session.Store(published.Select(p => ToProduct(p, busEvent.PublishDate)).ToArray());

        // Products that are no longer published drop out of the catalog.
        var skus = published.Select(p => p.Sku).ToArray();
        session.DeleteWhere<Product>(p => !p.Sku.IsOneOf(skus));

        await session.SaveChangesAsync();

        await bus.Publish(new CatalogUpdated { PublishDate = busEvent.PublishDate, ProductCount = published.Count });
    }

    private static Product ToProduct(PublishedProduct p, DateTimeOffset publishDate) => new()
    {
        Sku = p.Sku,
        Name = p.Name,
        Category = p.Category,
        Description = p.Description,
        Price = p.Price,
        Currency = p.Currency,
        ImageFileName = p.ImageFileName,
        Coffee = p.Coffee is not { } c ? null : new CoffeeDetails
        {
            Origin = c.Origin,
            Region = c.Region,
            Process = c.Process,
            Roast = c.Roast,
            WeightGrams = c.WeightGrams,
            TastingNotes = c.TastingNotes,
        },
        UpdatedAt = p.UpdatedAt,
        CatalogPublishedAt = publishDate,
    };
}

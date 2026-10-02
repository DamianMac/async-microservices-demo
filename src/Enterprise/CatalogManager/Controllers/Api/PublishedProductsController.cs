using CatalogManager.Data;
using CatalogManager.Domain;
using CatalogManager.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogManager.Controllers.Api;

// Lets other services fetch the published catalog after they're notified that it changed.
[ApiController]
[Route("api/products")]
public class PublishedProductsController(IDataContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<PublishedProduct>> Get(CancellationToken cancellationToken)
    {
        var products = await db.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Published && p.Price != null)
            .OrderBy(p => p.Sku)
            .ToListAsync(cancellationToken);

        return products.Select(ToPublished).ToList();
    }

    private PublishedProduct ToPublished(Product p) => new(
        p.Sku,
        p.Name,
        p.Category,
        p.Description,
        p.Price!.Value,
        "AUD",
        p.ImageUrl is null ? null : Path.GetFileName(p.ImageUrl),
        p.ImageUrl is null ? null : $"{Request.Scheme}://{Request.Host}{p.ImageUrl}",
        p.Coffee is not { } c ? null : new PublishedCoffeeDetails(c.Origin, c.Region, c.Process, c.Roast, c.WeightGrams, c.TastingNotes),
        p.UpdatedAt);
}

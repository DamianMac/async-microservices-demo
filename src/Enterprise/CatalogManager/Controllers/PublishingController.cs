using CatalogManager.Data;
using CatalogManager.Models;
using CatalogManager.Publishing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogManager.Controllers;

public class PublishingController(IDataContext db, CatalogPublisher publisher) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new PublishingViewModel
        {
            ToPublishCount = await db.Products.CountAsync(p => p.Price != null, cancellationToken),
            ChangedSincePublishCount = await db.Products.CountAsync(p => p.Price != null && (p.PublishedAt == null || p.UpdatedAt > p.PublishedAt), cancellationToken),
            NeedsPricingCount = await db.Products.CountAsync(p => p.Price == null, cancellationToken),
            History = await db.CatalogPublications.OrderByDescending(p => p.PublishedAt).Take(10).ToListAsync(cancellationToken)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(CancellationToken cancellationToken)
    {
        var publication = await publisher.PublishAsync(cancellationToken);
        TempData["Flash"] = $"Published {publication.ProductCount} products to the storefront.";
        return RedirectToAction(nameof(Index));
    }
}

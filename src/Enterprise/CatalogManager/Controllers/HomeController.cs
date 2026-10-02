using System.Diagnostics;
using CatalogManager.Data;
using CatalogManager.Domain;
using Microsoft.AspNetCore.Mvc;
using CatalogManager.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogManager.Controllers;

public class HomeController(IDataContext db) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new DashboardViewModel
        {
            ProductCount = await db.Products.CountAsync(cancellationToken),
            NeedsPricingCount = await db.Products.CountAsync(p => p.Price == null, cancellationToken),
            ReadyToPublishCount = await db.Products.CountAsync(p => p.Price != null && p.Status == ProductStatus.Draft, cancellationToken),
            PublishedCount = await db.Products.CountAsync(p => p.Status == ProductStatus.Published, cancellationToken),
            RecentProducts = await db.Products.OrderByDescending(p => p.UpdatedAt).ThenBy(p => p.Name).Take(8).ToListAsync(cancellationToken)
        };
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

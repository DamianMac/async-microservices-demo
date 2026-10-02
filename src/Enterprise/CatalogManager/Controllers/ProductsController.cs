using CatalogManager.Data;
using CatalogManager.Import;
using CatalogManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogManager.Controllers;

public class ProductsController(IDataContext db, ProductImporter importer, IConfiguration configuration) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var products = await db.Products
            .OrderBy(p => p.Category).ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null)
            return NotFound();

        return View(new ProductEditViewModel { Description = product.Description, Price = product.Price, Product = product });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductEditViewModel model, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.Product = product;
            return View(model);
        }

        product.Description = model.Description.Trim();
        product.Price = model.Price;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        TempData["Flash"] = $"Saved {product.Name}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Import()
    {
        var path = configuration.GetSection("Settings").GetValue<string>("ProductImportPath") ?? "";
        return View(new ImportViewModel { Path = path });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(ImportViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var result = await importer.ImportAsync(model.Path, cancellationToken);
            var message = $"Imported {result.Added + result.Updated} products: {result.Added} new, {result.Updated} updated, {result.ImagesCopied} images.";
            if (result.MissingImages.Count > 0)
                message += $" No image for {string.Join(", ", result.MissingImages)}.";
            TempData["Flash"] = message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidDataException or System.Text.Json.JsonException)
        {
            ModelState.AddModelError(nameof(model.Path), ex.Message);
            return View(model);
        }
    }
}

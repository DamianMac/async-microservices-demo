using Marten;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Domain;
using Warehouse.Import;
using Warehouse.Models;

namespace Warehouse.Controllers;

public class InventoryController(IQuerySession session, StockImporter importer, IConfiguration configuration) : Controller
{
    public async Task<IActionResult> Index(string? q, StockLevel? level, CancellationToken cancellationToken)
    {
        // The stock list is small, so it's filtered in memory.
        var all = await session.Query<StockItem>().ToListAsync(cancellationToken);

        var items = all.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            items = items.Where(i => i.Sku.Contains(term, StringComparison.OrdinalIgnoreCase)
                                     || i.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        if (level is not null)
            items = items.Where(i => i.Level() == level);

        return View(new InventoryViewModel
        {
            Items = items.OrderBy(i => i.Sku).ToList(),
            Query = q,
            Level = level,
            TotalCount = all.Count,
            LowCount = all.Count(i => i.Level() == StockLevel.Low),
            OutCount = all.Count(i => i.Level() == StockLevel.Out),
            MaxStock = all.Count == 0 ? 0 : all.Max(i => i.Stock)
        });
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
            TempData["Flash"] = $"Received {result.Added + result.Updated} SKUs: {result.Added} new, {result.Updated} updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidDataException or System.Text.Json.JsonException)
        {
            ModelState.AddModelError(nameof(model.Path), ex.Message);
            return View(model);
        }
    }
}

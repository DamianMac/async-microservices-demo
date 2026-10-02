using System.Diagnostics;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Domain;
using Warehouse.Models;

namespace Warehouse.Controllers;

public class HomeController(IQuerySession session) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await session.Query<StockItem>().ToListAsync(cancellationToken);
        var pickQueue = await session.Query<Dispatch>()
            .Where(d => d.Status == DispatchStatus.Pending)
            .OrderBy(d => d.ReceivedAt)
            .ToListAsync(cancellationToken);

        return View(new DashboardViewModel
        {
            SkuCount = items.Count,
            UnitsOnHand = items.Sum(i => Math.Max(i.Stock, 0)),
            LowCount = items.Count(i => i.Level() == StockLevel.Low),
            OutCount = items.Count(i => i.Level() == StockLevel.Out),
            WeightOnHandGrams = items.Sum(i => (long)Math.Max(i.Stock, 0) * (i.WeightGrams ?? 0)),
            NeedsReplenishment = items
                .Where(i => i.Level() != StockLevel.InStock)
                .OrderBy(i => i.Stock).ThenBy(i => i.Name)
                .ToList(),
            PickQueue = pickQueue.ToList()
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

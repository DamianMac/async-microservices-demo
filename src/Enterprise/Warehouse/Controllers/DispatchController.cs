using Marten;
using JasperFx;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Dispatching;
using Warehouse.Domain;
using Warehouse.Models;

namespace Warehouse.Controllers;

public class DispatchController(IQuerySession session, DispatchCompleter completer) : Controller
{
    // The pick queue (pending, oldest first) or the dispatched log (newest first).
    public async Task<IActionResult> Index(DispatchStatus status, CancellationToken cancellationToken)
    {
        var all = await session.Query<Dispatch>().ToListAsync(cancellationToken);

        var dispatches = status == DispatchStatus.Pending
            ? all.Where(d => d.Status == DispatchStatus.Pending).OrderBy(d => d.ReceivedAt)
            : all.Where(d => d.Status == DispatchStatus.Dispatched).OrderByDescending(d => d.DispatchedAt);

        return View(new DispatchListViewModel
        {
            Dispatches = dispatches.ToList(),
            Status = status,
            PendingCount = all.Count(d => d.Status == DispatchStatus.Pending),
            DispatchedCount = all.Count(d => d.Status == DispatchStatus.Dispatched),
        });
    }

    public async Task<IActionResult> Ticket(Guid id, CancellationToken cancellationToken)
    {
        var dispatch = await session.LoadAsync<Dispatch>(id, cancellationToken);
        if (dispatch == null)
            return NotFound();

        var skus = dispatch.PickList.Select(l => l.Sku).Distinct().ToArray();
        var stock = await session.LoadManyAsync<StockItem>(cancellationToken, skus);

        return View(new DispatchTicketViewModel
        {
            Dispatch = dispatch,
            Stock = stock.ToDictionary(i => i.Sku),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var reference = id.ToString("N")[..8].ToUpperInvariant();
        try
        {
            switch (await completer.Complete(id, cancellationToken))
            {
                case CompleteResult.NotFound:
                    return NotFound();
                case CompleteResult.AlreadyDispatched:
                    TempData["Flash"] = $"Order {reference} was already dispatched.";
                    return RedirectToAction(nameof(Ticket), new { id });
            }
        }
        catch (ConcurrencyException)
        {
            TempData["Flash"] = $"Order {reference} was completed by someone else.";
            return RedirectToAction(nameof(Ticket), new { id });
        }

        TempData["Flash"] = $"Order {reference} dispatched. Stock updated.";
        return RedirectToAction(nameof(Index));
    }
}

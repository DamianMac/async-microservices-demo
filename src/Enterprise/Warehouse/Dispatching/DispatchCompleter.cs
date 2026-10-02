using Marten;
using Marten.Patching;
using Nimbus.InfrastructureContracts;
using Warehouse.Domain;
using Warehouse.Messages;

namespace Warehouse.Dispatching;

public enum CompleteResult
{
    Completed,
    NotFound,
    AlreadyDispatched,
}

// Completes a dispatch: takes the picked units off stock, marks it dispatched, then tells
// Orders with OrderProcessed. Stock isn't checked, so it can go negative (shown as Out).
// Any SKU the pick takes to low or out of stock gets a LowStockWarning or OutOfStock.
public class DispatchCompleter(IDocumentSession session, IBus bus, StockAlerts stockAlerts, ILogger<DispatchCompleter> logger)
{
    public async Task<CompleteResult> Complete(Guid orderId, CancellationToken cancellationToken)
    {
        var dispatch = await session.LoadAsync<Dispatch>(orderId, cancellationToken);
        if (dispatch == null)
            return CompleteResult.NotFound;
        if (dispatch.Status != DispatchStatus.Pending)
            return CompleteResult.AlreadyDispatched;

        var skus = dispatch.PickList.Select(l => l.Sku).Distinct().ToArray();
        var known = (await session.LoadManyAsync<StockItem>(cancellationToken, skus)).Select(i => i.Sku).ToHashSet();
        var now = DateTime.UtcNow;

        // Patches decrement in the database, so two dispatches of the same SKU can't lose an update.
        foreach (var line in dispatch.PickList)
        {
            if (!known.Contains(line.Sku))
            {
                logger.LogWarning("Order {OrderId} picks {Sku}, which isn't in stock records; not decrementing it", orderId, line.Sku);
                continue;
            }
            session.Patch<StockItem>(line.Sku).Increment(i => i.Stock, -line.Quantity);
            session.Patch<StockItem>(line.Sku).Set(i => i.UpdatedAt, now);
        }

        dispatch.Status = DispatchStatus.Dispatched;
        dispatch.DispatchedAt = now;
        session.Store(dispatch);

        // Dispatch uses optimistic concurrency, so if two people complete it at once one of these throws.
        await session.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Dispatched order {OrderId}: {Units} units", orderId, dispatch.Units);

        // There's no outbox: if this fails the dispatch is done but Orders isn't told.
        await bus.Publish(new OrderProcessed { OrderId = orderId, DispatchedAt = now });

        await PublishStockAlerts(dispatch, known);
        return CompleteResult.Completed;
    }

    // The patches ran in the database, so reload to see the stock they left. The level before
    // this pick is that plus what was picked, so only a pick that changes the level alerts.
    private async Task PublishStockAlerts(Dispatch dispatch, HashSet<string> known)
    {
        var picked = dispatch.PickList
            .Where(l => known.Contains(l.Sku))
            .GroupBy(l => l.Sku)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));
        if (picked.Count == 0)
            return;

        var items = await session.LoadManyAsync<StockItem>(picked.Keys.ToArray());
        foreach (var item in items)
        {
            if (StockLevels.For(item.Stock + picked[item.Sku]) != item.Level())
                await stockAlerts.PublishFor(item);
        }
    }
}

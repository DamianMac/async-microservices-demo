using Warehouse.Domain;

namespace Warehouse.Models;

public class DispatchListViewModel
{
    public required IReadOnlyList<Dispatch> Dispatches { get; init; }
    public DispatchStatus Status { get; init; }
    public int PendingCount { get; init; }
    public int DispatchedCount { get; init; }
}

public class DispatchTicketViewModel
{
    public required Dispatch Dispatch { get; init; }
    // Stock on hand for each SKU on the pick list, missing if the SKU isn't on record.
    public required IReadOnlyDictionary<string, StockItem> Stock { get; init; }
}

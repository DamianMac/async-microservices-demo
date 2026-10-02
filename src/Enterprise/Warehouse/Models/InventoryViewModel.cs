using Warehouse.Domain;

namespace Warehouse.Models;

public class InventoryViewModel
{
    public required IReadOnlyList<StockItem> Items { get; init; }
    public string? Query { get; init; }
    public StockLevel? Level { get; init; }
    public int TotalCount { get; init; }
    public int LowCount { get; init; }
    public int OutCount { get; init; }
    // Scales the stock bars, so the fullest bin is a full bar.
    public int MaxStock { get; init; }
}

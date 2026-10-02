using Warehouse.Domain;

namespace Warehouse.Models;

public class DashboardViewModel
{
    public int SkuCount { get; init; }
    public int UnitsOnHand { get; init; }
    public int LowCount { get; init; }
    public int OutCount { get; init; }
    // Only items with a known weight count towards this.
    public long WeightOnHandGrams { get; init; }
    public required IReadOnlyList<StockItem> NeedsReplenishment { get; init; }
    // Oldest first.
    public required IReadOnlyList<Dispatch> PickQueue { get; init; }
}

using CatalogManager.Domain;

namespace CatalogManager.Models;

public class DashboardViewModel
{
    public int ProductCount { get; init; }
    public int NeedsPricingCount { get; init; }
    public int ReadyToPublishCount { get; init; }
    public int PublishedCount { get; init; }
    public IReadOnlyList<Product> RecentProducts { get; init; } = [];
}

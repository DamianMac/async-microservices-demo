using CatalogManager.Domain;

namespace CatalogManager.Models;

public class PublishingViewModel
{
    public int ToPublishCount { get; init; }
    public int ChangedSincePublishCount { get; init; }
    public int NeedsPricingCount { get; init; }
    public IReadOnlyList<CatalogPublication> History { get; init; } = [];
}

namespace CatalogManager.Models.Api;

public record PublishedProduct(
    string Sku,
    string Name,
    string Category,
    string Description,
    decimal Price,
    string Currency,
    string? ImageFileName,
    string? ImageUrl,
    PublishedCoffeeDetails? Coffee,
    DateTime UpdatedAt);

public record PublishedCoffeeDetails(
    string Origin,
    string Region,
    string Process,
    string Roast,
    int WeightGrams,
    IReadOnlyList<string> TastingNotes);

using Catalog.Domain;

namespace Catalog.Models;

public record ProductResponse(
    string Sku,
    string Name,
    string Category,
    string Description,
    decimal Price,
    string Currency,
    string? ImageUrl,
    CoffeeDetails? Coffee,
    DateTime UpdatedAt,
    StockLevel StockLevel);

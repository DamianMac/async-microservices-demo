namespace CatalogManager.Domain;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public CoffeeDetails? Coffee { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

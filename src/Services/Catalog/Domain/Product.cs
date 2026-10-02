namespace Catalog.Domain;

// Marten document. The SKU is the document id (see Program.cs).
public class Product
{
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string Currency { get; set; } = "";
    public string? ImageFileName { get; set; }
    public CoffeeDetails? Coffee { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTimeOffset CatalogPublishedAt { get; set; }
}

public class CoffeeDetails
{
    public string Origin { get; set; } = "";
    public string Region { get; set; } = "";
    public string Process { get; set; } = "";
    public string Roast { get; set; } = "";
    public int WeightGrams { get; set; }
    public List<string> TastingNotes { get; set; } = [];
}

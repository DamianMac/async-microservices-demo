namespace Orders.Domain;

// Orders' own copy of the published catalog's names and prices, refreshed on NewCatalogAvailable.
// Orders are priced from this rather than from whatever the shop sends.
public class Product
{
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string Currency { get; set; } = "AUD";
    public DateTime UpdatedAt { get; set; }
}

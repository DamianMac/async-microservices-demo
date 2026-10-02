namespace Search.Domain;

// The searchable text of a published product. Postgres builds the search vector from these columns.
public class Product
{
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Origin { get; set; }
    public string? Region { get; set; }
    public string? Process { get; set; }
    public string? Roast { get; set; }
    public string? TastingNotes { get; set; }
}

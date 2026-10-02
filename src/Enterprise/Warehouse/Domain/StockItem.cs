namespace Warehouse.Domain;

// Marten document. The SKU is the document id (see Program.cs).
public class StockItem
{
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public int Stock { get; set; }
    // Null when the product list gives no weight (equipment, for now).
    public int? WeightGrams { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

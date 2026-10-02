using System.Text.Json.Serialization;

namespace Catalog.Domain;

// Marten document: the warehouse's last word on a SKU's stock, from LowStockWarning and OutOfStock.
// Kept apart from Product, so storing a new catalog doesn't reset it, and the warehouse can
// report a SKU before it's published. A SKU with no status is in stock.
public class StockStatus
{
    public string Sku { get; set; } = "";
    public StockLevel Level { get; set; }
    public DateTime UpdatedAt { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<StockLevel>))]
public enum StockLevel
{
    InStock,
    Low,
    Out,
}

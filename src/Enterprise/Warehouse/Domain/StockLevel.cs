namespace Warehouse.Domain;

public enum StockLevel
{
    InStock,
    Low,
    Out
}

public static class StockLevels
{
    // At or below this many units an item needs replenishing.
    public const int LowStockThreshold = 10;

    public static StockLevel For(int stock) =>
        stock <= 0 ? StockLevel.Out
        : stock <= LowStockThreshold ? StockLevel.Low
        : StockLevel.InStock;

    public static StockLevel Level(this StockItem item) => For(item.Stock);
}

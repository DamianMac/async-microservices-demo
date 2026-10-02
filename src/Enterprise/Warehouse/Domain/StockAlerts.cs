using Nimbus.InfrastructureContracts;
using Warehouse.Messages;

namespace Warehouse.Domain;

// Tells the rest of the business when a SKU is running low or has run out.
public class StockAlerts(IBus bus, ILogger<StockAlerts> logger)
{
    // Publishes LowStockWarning or OutOfStock for the item's level; nothing when it's in stock.
    public async Task PublishFor(StockItem item)
    {
        switch (item.Level())
        {
            case StockLevel.Low:
                logger.LogInformation("{Sku} is low on stock ({Stock} units)", item.Sku, item.Stock);
                await bus.Publish(new LowStockWarning { Sku = item.Sku });
                break;
            case StockLevel.Out:
                logger.LogInformation("{Sku} is out of stock ({Stock} units)", item.Sku, item.Stock);
                await bus.Publish(new OutOfStock { Sku = item.Sku });
                break;
        }
    }
}

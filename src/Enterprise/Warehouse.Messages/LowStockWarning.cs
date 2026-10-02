using Nimbus.MessageContracts;

namespace Warehouse.Messages;

// Published by the warehouse when a SKU is at or below the low stock threshold
// (after a pick takes it there, and for every low SKU on each import).
public class LowStockWarning : IBusEvent
{
    public string Sku { get; set; } = "";
}

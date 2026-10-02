using Nimbus.MessageContracts;

namespace Warehouse.Messages;

// Published by the warehouse when a SKU has no stock left
// (after a pick takes it there, and for every empty SKU on each import).
public class OutOfStock : IBusEvent
{
    public string Sku { get; set; } = "";
}

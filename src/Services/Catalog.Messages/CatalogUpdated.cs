using Nimbus.MessageContracts;

namespace Catalog.Messages;

// Published by Catalog after it stores a new catalog. Subscribers fetch the products from GET /products.
public class CatalogUpdated : IBusEvent
{
    public DateTimeOffset PublishDate { get; set; }
    public int ProductCount { get; set; }
}

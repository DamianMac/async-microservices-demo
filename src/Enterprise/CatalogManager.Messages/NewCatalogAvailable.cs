using Nimbus.MessageContracts;

namespace CatalogManager.Messages;

public class NewCatalogAvailable : IBusEvent
{
    public DateTimeOffset PublishDate { get; set; }
}
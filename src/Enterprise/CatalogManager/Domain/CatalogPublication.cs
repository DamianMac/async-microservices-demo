namespace CatalogManager.Domain;

// A record of each time the catalog was published to the storefront.
public class CatalogPublication
{
    public int Id { get; set; }
    public DateTime PublishedAt { get; set; }
    public int ProductCount { get; set; }
}

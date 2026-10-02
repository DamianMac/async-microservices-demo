namespace Orders.CatalogManager;

// Fetches the published catalog from CatalogManager after it announces a new one.
public class CatalogManagerClient(HttpClient http)
{
    public async Task<IReadOnlyList<PublishedProduct>> GetPublishedProducts(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<PublishedProduct>>("api/products", cancellationToken) ?? [];
}

// The parts of CatalogManager's GET /api/products response that Orders prices from.
public record PublishedProduct(
    string Sku,
    string Name,
    decimal Price,
    string Currency);

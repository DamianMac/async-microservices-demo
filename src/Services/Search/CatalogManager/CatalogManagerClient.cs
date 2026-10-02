namespace Search.CatalogManager;

// Fetches the published catalog from CatalogManager after it announces a new one.
public class CatalogManagerClient(HttpClient http)
{
    public async Task<IReadOnlyList<PublishedProduct>> GetPublishedProducts(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<PublishedProduct>>("api/products", cancellationToken) ?? [];
}

// The parts of CatalogManager's GET /api/products response that Search indexes.
public record PublishedProduct(
    string Sku,
    string Name,
    string Category,
    string Description,
    PublishedCoffeeDetails? Coffee);

public record PublishedCoffeeDetails(
    string Origin,
    string Region,
    string Process,
    string Roast,
    List<string> TastingNotes);

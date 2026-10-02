namespace Catalog.CatalogManager;

// Fetches the published catalog from CatalogManager after it announces a new one.
public class CatalogManagerClient(HttpClient http)
{
    public async Task<IReadOnlyList<PublishedProduct>> GetPublishedProducts(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<PublishedProduct>>("api/products", cancellationToken) ?? [];

    // Downloads to a temp file, then moves it into place, so concurrent handlers
    // don't collide and the image is never served half-written.
    public async Task DownloadTo(string url, string path, CancellationToken cancellationToken = default)
    {
        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var source = await http.GetStreamAsync(url, cancellationToken))
            await using (var target = File.Create(tempPath))
            {
                await source.CopyToAsync(target, cancellationToken);
            }
            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}

// Mirrors the response of CatalogManager's GET /api/products.
public record PublishedProduct(
    string Sku,
    string Name,
    string Category,
    string Description,
    decimal Price,
    string Currency,
    string? ImageFileName,
    string? ImageUrl,
    PublishedCoffeeDetails? Coffee,
    DateTime UpdatedAt);

public record PublishedCoffeeDetails(
    string Origin,
    string Region,
    string Process,
    string Roast,
    int WeightGrams,
    List<string> TastingNotes);

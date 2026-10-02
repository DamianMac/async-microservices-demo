namespace Catalog.Images;

// Where downloaded product images live on disk and the URL they're served from.
// Instances run from the same project folder, so they share the images.
public class ProductImages(IWebHostEnvironment env, IConfiguration config)
{
    public const string RequestPath = "/images/products";

    public string Folder { get; } = Path.Combine(env.ContentRootPath, "wwwroot", "images", "products");

    public string PathFor(string fileName) => Path.Combine(Folder, Path.GetFileName(fileName));

    public string UrlFor(string fileName) =>
        $"{config.GetSection("Settings").GetValue<string>("PublicBaseUrl")!.TrimEnd('/')}{RequestPath}/{Uri.EscapeDataString(fileName)}";
}

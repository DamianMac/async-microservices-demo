using System.Text.Json;
using CatalogManager.Data;
using CatalogManager.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogManager.Import;

public record ImportResult(int Added, int Updated, int ImagesCopied, IReadOnlyList<string> MissingImages);

// Imports the product list JSON (data/products.json) into the catalog, matching on SKU.
// Each product's rendered PNG, from images/rendered next to the JSON file, is copied
// into wwwroot/images/products so the app can serve it.
public class ProductImporter(IDataContext db, IWebHostEnvironment env, ILogger<ProductImporter> logger)
{
    public const string ImageFolder = "images/products";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<ImportResult> ImportAsync(string jsonPath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(jsonPath, env.ContentRootPath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"No product list found at {fullPath}", fullPath);

        await using var stream = File.OpenRead(fullPath);
        var file = await JsonSerializer.DeserializeAsync<ProductFile>(stream, JsonOptions, cancellationToken)
                   ?? throw new InvalidDataException($"{fullPath} is empty");

        var renderedDir = Path.Combine(Path.GetDirectoryName(fullPath)!, "images", "rendered");
        var imageDir = Path.Combine(env.WebRootPath, ImageFolder);
        Directory.CreateDirectory(imageDir);

        var existing = await db.Products.ToDictionaryAsync(p => p.Sku, cancellationToken);
        var now = DateTime.UtcNow;
        int added = 0, updated = 0, imagesCopied = 0;
        var missingImages = new List<string>();

        foreach (var item in file.Products)
        {
            var imageName = Path.GetFileNameWithoutExtension(item.Image) + ".png";
            var source = Path.Combine(renderedDir, imageName);
            string? imageUrl = null;
            if (File.Exists(source))
            {
                File.Copy(source, Path.Combine(imageDir, imageName), overwrite: true);
                imageUrl = $"/{ImageFolder}/{imageName}";
                imagesCopied++;
            }
            else
            {
                missingImages.Add(item.Sku);
            }

            if (!existing.TryGetValue(item.Sku, out var product))
            {
                // New products take the list price; after that the price is managed here,
                // so a re-import doesn't overwrite it.
                product = new Product { Sku = item.Sku, Price = item.Price, CreatedAt = now };
                db.Products.Add(product);
                added++;
            }
            else
            {
                updated++;
            }

            product.Name = item.Name;
            product.Category = item.Category;
            product.Description = item.Description;
            product.ImageUrl = imageUrl ?? product.ImageUrl;
            product.Coffee = item.Coffee is null ? null : new CoffeeDetails
            {
                Origin = item.Coffee.Origin,
                Region = item.Coffee.Region,
                Process = item.Coffee.Process,
                Roast = item.Coffee.Roast,
                WeightGrams = item.Coffee.WeightGrams,
                TastingNotes = item.Coffee.TastingNotes.ToList()
            };
            product.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Imported {Count} products from {Path}: {Added} added, {Updated} updated, {ImagesCopied} images copied",
            file.Products.Count, fullPath, added, updated, imagesCopied);
        if (missingImages.Count > 0)
            logger.LogWarning("No rendered image for {Skus}", missingImages);

        return new ImportResult(added, updated, imagesCopied, missingImages);
    }

    private record ProductFile(string Currency, List<ProductItem> Products);

    private record ProductItem(
        string Sku,
        string Name,
        string Category,
        string Description,
        string Image,
        decimal Price,
        CoffeeItem? Coffee);

    private record CoffeeItem(
        string Origin,
        string Region,
        string Process,
        string Roast,
        int WeightGrams,
        List<string> TastingNotes);
}

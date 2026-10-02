using System.Globalization;

namespace CatalogManager;

public static class Formats
{
    // The catalog is priced in AUD (see data/products.json).
    public static readonly CultureInfo Currency = CultureInfo.GetCultureInfo("en-AU");
}

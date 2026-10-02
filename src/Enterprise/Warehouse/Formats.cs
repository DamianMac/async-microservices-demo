using System.Globalization;

namespace Warehouse;

public static class Formats
{
    public static string Weight(int? grams) => grams switch
    {
        null => "—",
        >= 1000 => (grams.Value / 1000m).ToString("0.##", CultureInfo.InvariantCulture) + " kg",
        _ => grams.Value.ToString("N0", CultureInfo.InvariantCulture) + " g"
    };

    public static string Kilograms(long grams) =>
        (grams / 1000m).ToString("N1", CultureInfo.InvariantCulture) + " kg";

    // "just now", "5 min ago", "3 h ago", then the date.
    public static string Ago(DateTime utc)
    {
        var age = DateTime.UtcNow - utc;
        return age.TotalMinutes < 1 ? "just now"
            : age.TotalHours < 1 ? $"{(int)age.TotalMinutes} min ago"
            : age.TotalDays < 1 ? $"{(int)age.TotalHours} h ago"
            : utc.ToLocalTime().ToString("d MMM", CultureInfo.InvariantCulture);
    }
}

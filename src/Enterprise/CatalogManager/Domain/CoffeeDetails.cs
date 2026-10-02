namespace CatalogManager.Domain;

public class CoffeeDetails
{
    public string Origin { get; set; } = "";
    public string Region { get; set; } = "";
    public string Process { get; set; } = "";
    public string Roast { get; set; } = "";
    public int WeightGrams { get; set; }
    public List<string> TastingNotes { get; set; } = [];
}

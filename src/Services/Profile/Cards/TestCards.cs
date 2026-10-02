namespace Profile.Cards;

// The shop only takes well-known test card numbers, so nobody can save a real card by accident.
// Nothing is charged; PaymentGateway will pretend.
public static class TestCards
{
    private static readonly Dictionary<string, string> Brands = new()
    {
        ["4242424242424242"] = "Visa",
        ["4000056655665556"] = "Visa",
        ["5555555555554444"] = "Mastercard",
        ["2223003122003222"] = "Mastercard",
        ["378282246310005"] = "Amex",
        ["6011111111111117"] = "Discover",
    };

    public static IEnumerable<(string Number, string Brand)> All => Brands.Select(b => (b.Key, b.Value));

    public static string Normalise(string number) => new(number.Where(char.IsDigit).ToArray());

    public static bool TryGetBrand(string number, out string brand) =>
        Brands.TryGetValue(Normalise(number), out brand!);
}

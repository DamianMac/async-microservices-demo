namespace Orders.Pricing;

// Matches the shop's "Free shipping on orders over $60" banner. The shop shows the same rule
// in the cart, but the order is charged what this says.
public static class Shipping
{
    public const decimal FreeFrom = 60m;
    public const decimal FlatRate = 9.95m;

    public static decimal For(decimal subtotal) => subtotal >= FreeFrom ? 0m : FlatRate;
}

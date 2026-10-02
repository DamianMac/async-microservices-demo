namespace Profile.Domain;

// A pretend saved card. Only the brand, last four digits and expiry are kept;
// the full number and CVC never reach the database.
public class PaymentCard
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CardholderName { get; set; } = "";
    public string Brand { get; set; } = "";
    public string Last4 { get; set; } = "";
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

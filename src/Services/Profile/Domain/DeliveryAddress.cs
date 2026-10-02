namespace Profile.Domain;

public class DeliveryAddress
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Label { get; set; } = "";
    public string RecipientName { get; set; } = "";
    public string Line1 { get; set; } = "";
    public string? Line2 { get; set; }
    public string Suburb { get; set; } = "";
    public string State { get; set; } = "";
    public string Postcode { get; set; } = "";
    public string Country { get; set; } = "Australia";
    public string? Phone { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace Orders.Messages;

public class DeliveryAddress
{
    public string RecipientName { get; set; } = "";
    public string Line1 { get; set; } = "";
    public string? Line2 { get; set; }
    public string Suburb { get; set; } = "";
    public string State { get; set; } = "";
    public string Postcode { get; set; } = "";
    public string Country { get; set; } = "";
    public string? Phone { get; set; }
}

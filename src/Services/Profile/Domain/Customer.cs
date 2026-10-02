namespace Profile.Domain;

public class Customer
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<DeliveryAddress> Addresses { get; set; } = [];
    public List<PaymentCard> Cards { get; set; } = [];
}

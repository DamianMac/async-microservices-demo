using System.ComponentModel.DataAnnotations;
using Profile.Domain;

namespace Profile.Models;

public class CardRequest
{
    [Required, MaxLength(200)] public string CardholderName { get; set; } = "";
    [Required] public string Number { get; set; } = "";
    [Range(1, 12)] public int ExpiryMonth { get; set; }
    [Range(2000, 2100)] public int ExpiryYear { get; set; }
    // Checked for shape, then thrown away.
    [Required, RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVC must be 3 or 4 digits.")] public string Cvc { get; set; } = "";
    public bool IsDefault { get; set; }
}

public record CardResponse(int Id, string CardholderName, string Brand, string Last4, int ExpiryMonth, int ExpiryYear, bool IsDefault)
{
    public static CardResponse From(PaymentCard c) =>
        new(c.Id, c.CardholderName, c.Brand, c.Last4, c.ExpiryMonth, c.ExpiryYear, c.IsDefault);
}

public record TestCardResponse(string Number, string Brand);

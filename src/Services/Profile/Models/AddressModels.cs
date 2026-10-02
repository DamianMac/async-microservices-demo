using System.ComponentModel.DataAnnotations;
using Profile.Domain;

namespace Profile.Models;

public class AddressRequest
{
    [Required, MaxLength(50)] public string Label { get; set; } = "";
    [Required, MaxLength(200)] public string RecipientName { get; set; } = "";
    [Required, MaxLength(200)] public string Line1 { get; set; } = "";
    [MaxLength(200)] public string? Line2 { get; set; }
    [Required, MaxLength(100)] public string Suburb { get; set; } = "";
    [Required, MaxLength(50)] public string State { get; set; } = "";
    [Required, RegularExpression(@"^\d{4}$", ErrorMessage = "Postcode must be 4 digits.")] public string Postcode { get; set; } = "";
    [MaxLength(100)] public string Country { get; set; } = "Australia";
    [MaxLength(30)] public string? Phone { get; set; }
    public bool IsDefault { get; set; }

    public void ApplyTo(DeliveryAddress address)
    {
        address.Label = Label.Trim();
        address.RecipientName = RecipientName.Trim();
        address.Line1 = Line1.Trim();
        address.Line2 = string.IsNullOrWhiteSpace(Line2) ? null : Line2.Trim();
        address.Suburb = Suburb.Trim();
        address.State = State.Trim();
        address.Postcode = Postcode.Trim();
        address.Country = string.IsNullOrWhiteSpace(Country) ? "Australia" : Country.Trim();
        address.Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim();
    }
}

public record AddressResponse(
    int Id, string Label, string RecipientName, string Line1, string? Line2,
    string Suburb, string State, string Postcode, string Country, string? Phone, bool IsDefault)
{
    public static AddressResponse From(DeliveryAddress a) =>
        new(a.Id, a.Label, a.RecipientName, a.Line1, a.Line2, a.Suburb, a.State, a.Postcode, a.Country, a.Phone, a.IsDefault);
}

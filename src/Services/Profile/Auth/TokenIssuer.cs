using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Profile.Domain;

namespace Profile.Auth;

// Issues the JWT the shop sends as a bearer token. It's signed with a shared symmetric key
// (Settings:Jwt), so another service could validate it with the same settings.
public class TokenIssuer(JwtSettings settings)
{
    public string Issue(Customer customer)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, customer.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, customer.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, customer.LastName),
        };

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(settings.LifetimeDays),
            signingCredentials: new SigningCredentials(settings.SigningKey(), SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class JwtSettings
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
    public int LifetimeDays { get; set; } = 7;

    public SymmetricSecurityKey SigningKey() => new(Encoding.UTF8.GetBytes(Key));
}

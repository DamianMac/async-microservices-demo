using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Orders.Auth;

// Profile issues the shop's JWT. Orders validates it with the same issuer, audience and shared key.
public class JwtSettings
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";

    public SymmetricSecurityKey SigningKey() => new(Encoding.UTF8.GetBytes(Key));
}

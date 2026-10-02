using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Profile.Auth;

public static class ClaimsPrincipalExtensions
{
    // Inbound claim mapping is off, so the subject arrives as "sub".
    public static int CustomerId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}

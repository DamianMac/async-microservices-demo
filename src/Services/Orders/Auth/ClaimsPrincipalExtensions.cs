using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Orders.Auth;

public static class ClaimsPrincipalExtensions
{
    // Inbound claim mapping is off, so the claims keep their JWT names.
    public static int CustomerId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    public static string Email(this ClaimsPrincipal user) =>
        user.FindFirstValue(JwtRegisteredClaimNames.Email) ?? "";

    public static string FirstName(this ClaimsPrincipal user) =>
        user.FindFirstValue(JwtRegisteredClaimNames.GivenName) ?? "";

    public static string LastName(this ClaimsPrincipal user) =>
        user.FindFirstValue(JwtRegisteredClaimNames.FamilyName) ?? "";
}

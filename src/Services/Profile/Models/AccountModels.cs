using System.ComponentModel.DataAnnotations;
using Profile.Domain;

namespace Profile.Models;

public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(256)] public string Email { get; set; } = "";
    [Required, MinLength(8), MaxLength(100)] public string Password { get; set; } = "";
    [Required, MaxLength(100)] public string FirstName { get; set; } = "";
    [Required, MaxLength(100)] public string LastName { get; set; } = "";
}

public class LoginRequest
{
    [Required] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public class UpdateAccountRequest
{
    [Required, MaxLength(100)] public string FirstName { get; set; } = "";
    [Required, MaxLength(100)] public string LastName { get; set; } = "";
}

public record AccountResponse(int Id, string Email, string FirstName, string LastName, DateTime CreatedAt)
{
    public static AccountResponse From(Customer c) => new(c.Id, c.Email, c.FirstName, c.LastName, c.CreatedAt);
}

public record AuthResponse(string Token, AccountResponse Account);

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nimbus.InfrastructureContracts;
using Profile.Auth;
using Profile.Data;
using Profile.Domain;
using Profile.Messages;
using Profile.Models;

namespace Profile.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IDataContext db, IPasswordHasher<Customer> hasher, TokenIssuer tokens, IBus bus, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Customers.AnyAsync(c => c.Email == email))
        {
            ModelState.AddModelError(nameof(request.Email), "An account with that email already exists.");
            return ValidationProblem(ModelState);
        }

        var now = DateTime.UtcNow;
        var customer = new Customer
        {
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };
        customer.PasswordHash = hasher.HashPassword(customer, request.Password);

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        await bus.Publish(new CustomerRegistered
        {
            CustomerId = customer.Id,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            RegisteredAt = new DateTimeOffset(now),
        });

        logger.LogInformation("Registered customer {CustomerId} ({Email}) and sent {Event}", customer.Id, customer.Email, nameof(CustomerRegistered));
        return new AuthResponse(tokens.Issue(customer), AccountResponse.From(customer));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var customer = await db.Customers.SingleOrDefaultAsync(c => c.Email == email);

        var result = customer == null
            ? PasswordVerificationResult.Failed
            : hasher.VerifyHashedPassword(customer, customer.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            logger.LogInformation("Failed login for {Email}", email);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Email or password is incorrect.");
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            customer!.PasswordHash = hasher.HashPassword(customer, request.Password);
            await db.SaveChangesAsync();
        }

        logger.LogInformation("Customer {CustomerId} logged in", customer!.Id);
        return new AuthResponse(tokens.Issue(customer), AccountResponse.From(customer));
    }
}

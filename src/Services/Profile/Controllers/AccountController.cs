using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Profile.Auth;
using Profile.Data;
using Profile.Models;

namespace Profile.Controllers;

[ApiController]
[Authorize]
[Route("account")]
public class AccountController(IDataContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AccountResponse>> Get()
    {
        var customer = await db.Customers.SingleOrDefaultAsync(c => c.Id == User.CustomerId());
        // The token can outlive the account, e.g. after ./run.sh reset.
        return customer == null ? Unauthorized() : AccountResponse.From(customer);
    }

    [HttpPut]
    public async Task<ActionResult<AccountResponse>> Update(UpdateAccountRequest request)
    {
        var customer = await db.Customers.SingleOrDefaultAsync(c => c.Id == User.CustomerId());
        if (customer == null) return Unauthorized();

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return AccountResponse.From(customer);
    }
}

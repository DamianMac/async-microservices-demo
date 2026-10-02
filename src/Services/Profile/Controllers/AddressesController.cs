using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Profile.Auth;
using Profile.Data;
using Profile.Domain;
using Profile.Models;

namespace Profile.Controllers;

// The signed-in customer's delivery addresses. One is the default: the first one added,
// then whichever is marked default, and the oldest remaining one if the default is deleted.
[ApiController]
[Authorize]
[Route("addresses")]
public class AddressesController(IDataContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<AddressResponse>> List()
    {
        var addresses = await Mine().OrderByDescending(a => a.IsDefault).ThenBy(a => a.Id).ToListAsync();
        return addresses.Select(AddressResponse.From);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AddressResponse>> Get(int id)
    {
        var address = await Mine().SingleOrDefaultAsync(a => a.Id == id);
        return address == null ? NotFound() : AddressResponse.From(address);
    }

    [HttpPost]
    public async Task<ActionResult<AddressResponse>> Create(AddressRequest request)
    {
        var existing = await Mine().ToListAsync();
        var now = DateTime.UtcNow;
        var address = new DeliveryAddress { CustomerId = User.CustomerId(), CreatedAt = now, UpdatedAt = now };
        request.ApplyTo(address);

        if (request.IsDefault || existing.Count == 0)
            MakeDefault(address, existing);

        db.DeliveryAddresses.Add(address);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = address.Id }, AddressResponse.From(address));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AddressResponse>> Update(int id, AddressRequest request)
    {
        var addresses = await Mine().ToListAsync();
        var address = addresses.SingleOrDefault(a => a.Id == id);
        if (address == null) return NotFound();

        request.ApplyTo(address);
        address.UpdatedAt = DateTime.UtcNow;
        if (request.IsDefault)
            MakeDefault(address, addresses);

        await db.SaveChangesAsync();
        return AddressResponse.From(address);
    }

    [HttpPost("{id:int}/default")]
    public async Task<ActionResult<AddressResponse>> SetDefault(int id)
    {
        var addresses = await Mine().ToListAsync();
        var address = addresses.SingleOrDefault(a => a.Id == id);
        if (address == null) return NotFound();

        MakeDefault(address, addresses);
        await db.SaveChangesAsync();
        return AddressResponse.From(address);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var addresses = await Mine().OrderBy(a => a.Id).ToListAsync();
        var address = addresses.SingleOrDefault(a => a.Id == id);
        if (address == null) return NotFound();

        db.DeliveryAddresses.Remove(address);
        if (address.IsDefault && addresses.FirstOrDefault(a => a != address) is { } next)
            next.IsDefault = true;

        await db.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<DeliveryAddress> Mine()
    {
        var customerId = User.CustomerId();
        return db.DeliveryAddresses.Where(a => a.CustomerId == customerId);
    }

    private static void MakeDefault(DeliveryAddress address, IEnumerable<DeliveryAddress> all)
    {
        foreach (var other in all)
            other.IsDefault = false;
        address.IsDefault = true;
    }
}

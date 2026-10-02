using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Profile.Auth;
using Profile.Cards;
using Profile.Data;
using Profile.Domain;
using Profile.Models;

namespace Profile.Controllers;

// The signed-in customer's saved (fake) cards. Only test card numbers are accepted, and only
// the brand, last four digits and expiry are stored. Default handling matches addresses.
[ApiController]
[Authorize]
[Route("cards")]
public class CardsController(IDataContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<CardResponse>> List()
    {
        var cards = await Mine().OrderByDescending(c => c.IsDefault).ThenBy(c => c.Id).ToListAsync();
        return cards.Select(CardResponse.From);
    }

    // So the shop can offer them on the add card form.
    [HttpGet("test-numbers")]
    [AllowAnonymous]
    public IEnumerable<TestCardResponse> TestNumbers() => TestCards.All.Select(c => new TestCardResponse(c.Number, c.Brand));

    [HttpPost]
    public async Task<ActionResult<CardResponse>> Create(CardRequest request)
    {
        if (!TestCards.TryGetBrand(request.Number, out var brand))
            ModelState.AddModelError(nameof(request.Number), "Only test card numbers are accepted, e.g. 4242 4242 4242 4242.");

        var now = DateTime.UtcNow;
        if (request.ExpiryYear < now.Year || (request.ExpiryYear == now.Year && request.ExpiryMonth < now.Month))
            ModelState.AddModelError(nameof(request.ExpiryYear), "The card has expired.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var existing = await Mine().ToListAsync();
        var number = TestCards.Normalise(request.Number);
        var card = new PaymentCard
        {
            CustomerId = User.CustomerId(),
            CardholderName = request.CardholderName.Trim(),
            Brand = brand,
            Last4 = number[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            CreatedAt = now,
        };

        if (request.IsDefault || existing.Count == 0)
            MakeDefault(card, existing);

        db.PaymentCards.Add(card);
        await db.SaveChangesAsync();

        return Created($"cards/{card.Id}", CardResponse.From(card));
    }

    [HttpPost("{id:int}/default")]
    public async Task<ActionResult<CardResponse>> SetDefault(int id)
    {
        var cards = await Mine().ToListAsync();
        var card = cards.SingleOrDefault(c => c.Id == id);
        if (card == null) return NotFound();

        MakeDefault(card, cards);
        await db.SaveChangesAsync();
        return CardResponse.From(card);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cards = await Mine().OrderBy(c => c.Id).ToListAsync();
        var card = cards.SingleOrDefault(c => c.Id == id);
        if (card == null) return NotFound();

        db.PaymentCards.Remove(card);
        if (card.IsDefault && cards.FirstOrDefault(c => c != card) is { } next)
            next.IsDefault = true;

        await db.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<PaymentCard> Mine()
    {
        var customerId = User.CustomerId();
        return db.PaymentCards.Where(c => c.CustomerId == customerId);
    }

    private static void MakeDefault(PaymentCard card, IEnumerable<PaymentCard> all)
    {
        foreach (var other in all)
            other.IsDefault = false;
        card.IsDefault = true;
    }
}

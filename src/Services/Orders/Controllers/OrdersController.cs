using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nimbus.InfrastructureContracts;
using Orders.Auth;
using Orders.Data;
using Orders.Domain;
using Orders.Models;
using Orders.Messages;
using Orders.Pricing;

namespace Orders.Controllers;

[ApiController]
[Authorize]
[Route("orders")]
public class OrdersController(IDataContext db, IBus bus, ILogger<OrdersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<OrderSummaryResponse>> List()
    {
        var orders = await Mine().Include(o => o.Lines).OrderByDescending(o => o.PlacedAt).ToListAsync();
        return orders.Select(OrderSummaryResponse.From);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> Get(Guid id)
    {
        var order = await Mine().Include(o => o.Lines).SingleOrDefaultAsync(o => o.Id == id);
        return order == null ? NotFound() : OrderResponse.From(order);
    }

    // Creates a pending order, priced from Orders' copy of the published catalog.
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Place(PlaceOrderRequest request)
    {
        var id = request.Id!.Value;
        var customerId = User.CustomerId();

        // A repeat of an order that's already been placed gets the same order back.
        var existing = await db.Orders.Include(o => o.Lines).SingleOrDefaultAsync(o => o.Id == id);
        if (existing != null)
            return existing.CustomerId == customerId ? OrderResponse.From(existing) : Conflict();

        var quantities = request.Items
            .GroupBy(i => i.Sku)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
        var skus = quantities.Keys.ToList();
        var products = await db.Products.Where(p => skus.Contains(p.Sku)).ToDictionaryAsync(p => p.Sku);

        var unavailable = skus.Where(s => !products.ContainsKey(s)).ToList();
        if (unavailable.Count > 0)
        {
            ModelState.AddModelError(nameof(request.Items), $"No longer available: {string.Join(", ", unavailable)}.");
            return ValidationProblem();
        }

        var lines = request.Items
            .Select(i => i.Sku).Distinct()
            .Select(sku => new OrderLine
            {
                Sku = sku,
                Name = products[sku].Name,
                UnitPrice = products[sku].Price,
                Quantity = quantities[sku],
                LineTotal = products[sku].Price * quantities[sku],
            })
            .ToList();

        var subtotal = lines.Sum(l => l.LineTotal);
        var shipping = Shipping.For(subtotal);
        var now = DateTime.UtcNow;

        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            CustomerEmail = User.Email(),
            CustomerFirstName = User.FirstName(),
            CustomerLastName = User.LastName(),
            Status = OrderStatus.Pending,
            DeliveryAddress = request.DeliveryAddress!.ToAddress(),
            PaymentCardId = request.PaymentCard!.Id,
            CardBrand = request.PaymentCard.Brand,
            CardLast4 = request.PaymentCard.Last4,
            Subtotal = subtotal,
            Shipping = shipping,
            Total = subtotal + shipping,
            Currency = products[lines[0].Sku].Currency,
            PlacedAt = now,
            UpdatedAt = now,
            Lines = lines,
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        logger.LogInformation("Customer {CustomerId} placed order {OrderId} for {Total} {Currency}", customerId, order.Id, order.Total, order.Currency);

        await bus.Send(new ChargeCustomerCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Amount = order.Total,
            Currency = order.Currency,
            PaymentCardId = order.PaymentCardId,
            CardBrand = order.CardBrand,
            CardLast4 = order.CardLast4,
            PlacedAt = order.PlacedAt,
        });

        return CreatedAtAction(nameof(Get), new { id = order.Id }, OrderResponse.From(order));
    }

    private IQueryable<Order> Mine()
    {
        var customerId = User.CustomerId();
        return db.Orders.Where(o => o.CustomerId == customerId);
    }
}

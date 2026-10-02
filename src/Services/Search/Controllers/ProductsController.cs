using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Search.Data;
using Search.Data.Mapping;
using Search.Models;

namespace Search.Controllers;

[ApiController]
[Route("products")]
public class ProductsController(IDataContext db) : ControllerBase
{
    // GET /products?q=fruity light roast - web search syntax, so "quoted phrases", OR and -exclusions work.
    [HttpGet]
    public async Task<IReadOnlyList<SearchResult>> Get([FromQuery] string? q, CancellationToken cancellationToken, [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return [];

        return await db.Products
            .Where(p => EF.Property<NpgsqlTsVector>(p, ProductMapping.SearchVector).Matches(EF.Functions.WebSearchToTsQuery("english", q)))
            .OrderByDescending(p => EF.Property<NpgsqlTsVector>(p, ProductMapping.SearchVector).Rank(EF.Functions.WebSearchToTsQuery("english", q)))
            .ThenBy(p => p.Sku)
            .Take(Math.Clamp(limit, 1, 100))
            .Select(p => new SearchResult(
                p.Sku,
                p.Name,
                p.Category,
                EF.Property<NpgsqlTsVector>(p, ProductMapping.SearchVector).Rank(EF.Functions.WebSearchToTsQuery("english", q))))
            .ToListAsync(cancellationToken);
    }
}

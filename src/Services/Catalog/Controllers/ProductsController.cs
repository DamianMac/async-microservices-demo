using Catalog.Caching;
using Catalog.Models;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Controllers;

[ApiController]
[Route("products")]
public class ProductsController(ProductCache cache) : ControllerBase
{
    [HttpGet]
    public IReadOnlyList<ProductResponse> Get() => cache.Products;
}

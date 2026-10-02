using Microsoft.AspNetCore.Mvc;

namespace Orders.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { service = "Orders" });
}

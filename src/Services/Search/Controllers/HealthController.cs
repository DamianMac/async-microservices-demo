using Microsoft.AspNetCore.Mvc;

namespace Search.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { service = "Search" });
}

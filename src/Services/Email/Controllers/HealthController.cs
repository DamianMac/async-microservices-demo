using Microsoft.AspNetCore.Mvc;

namespace Email.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { service = "Email" });
}

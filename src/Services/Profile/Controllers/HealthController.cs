using Microsoft.AspNetCore.Mvc;

namespace Profile.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { service = "Profile" });
}

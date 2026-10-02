using Microsoft.AspNetCore.Mvc;

namespace PaymentGateway.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { service = "PaymentGateway" });
}

using Microsoft.AspNetCore.Mvc;

namespace PayFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get() =>
        Ok(new
        {
            status = "Healthy",
            service = "PayFlow.Api",
            utc = DateTime.UtcNow
        });
}

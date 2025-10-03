using Microsoft.AspNetCore.Mvc;

namespace Todo.MainApi.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("healthy");
}

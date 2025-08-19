using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/health")]
    [ApiVersion("1.0")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { status = "ok" });
    }
}

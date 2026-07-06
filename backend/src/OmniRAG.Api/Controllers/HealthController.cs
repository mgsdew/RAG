using Microsoft.AspNetCore.Mvc;

namespace OmniRAG.Api.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public HealthController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet("status")]
        public IActionResult Get()
        {
            return Ok(new { status = "running", environment = _environment.EnvironmentName });
        }

        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            return Ok(new { service = "OmniRAG.API", version = "0.1.0" });
        }
    }
}

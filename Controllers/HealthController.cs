using Microsoft.AspNetCore.Mvc;

namespace EcommerceWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Health check endpoint for container liveness/readiness probes.
        /// </summary>
        /// <returns>200 OK with status payload when the service is healthy.</returns>
        [HttpGet]
        [Route("/health")]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }
    }
}

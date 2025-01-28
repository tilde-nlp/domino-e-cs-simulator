using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace DominoCS.Controllers
{
    [Route("health")]
    public class HealthController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Healthy");
        }
    }
}
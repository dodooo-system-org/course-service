using Microsoft.AspNetCore.Mvc;

namespace course_service.Modules.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello, World!");
        }
    }
}
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace course_service.Modules.App.Controllers
{
    [ApiController]
    [Route("api/app")]
    public class AppController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            return Ok("Hello, World!");
        }
    }
}
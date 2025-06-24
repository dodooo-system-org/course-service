using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using course_service.Modules.LessonPart.DTOs;
using course_service.Modules.LessonPart.Services;

namespace course_service.Modules.LessonPart.Controllers
{
    [Route("api/lesson-part")]
    [ApiController]
    public class LessonPartController : ControllerBase
    {
        private readonly LessonPartService _lessonPartService;

        public LessonPartController(LessonPartService lessonPartService)
        {
            _lessonPartService = lessonPartService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateLessonPart([FromBody] CreateLessonPartDto lessonPartDto)
        {
            var createdLessonPart = await _lessonPartService.CreateOneAsync(lessonPartDto);
            return Ok(createdLessonPart);
        }

        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetLessonPartsByLesson(Guid lessonId)
        {
            var lessonParts = await _lessonPartService.GetAllAsync(lessonId);
            return Ok(lessonParts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLessonPart(Guid id)
        {
            var lessonPart = await _lessonPartService.GetOneAsync(id);
            return Ok(lessonPart);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLessonPart(Guid id, [FromBody] UpdateLessonPartDto lessonPartDto)
        {
            var updatedLessonPart = await _lessonPartService.UpdateOneAsync(id, lessonPartDto);
            return Ok(updatedLessonPart);
        }
    }
}

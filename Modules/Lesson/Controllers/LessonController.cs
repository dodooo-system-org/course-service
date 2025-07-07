using course_service.Attributes;
using course_service.Modules.Lesson.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace course_service.Modules.Lesson.Controllers
{
    [Route("api/lesson")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly LessonService _lessonService;
        public LessonController(LessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLessons()
        {
            var lessons = await _lessonService.GetAllAsync();
            Console.WriteLine(lessons.Count());
            return Ok(lessons);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLesson(Guid id)
        {
            var lesson = await _lessonService.GetOneByIdAsync(id);
            return Ok(lesson);
        }

        [HttpPost]
        [RequireAdmin]
        public async Task<IActionResult> CreateLesson([FromBody] CreateLessonDto lesson)
        {
            var createdLesson = await _lessonService.CreateOneAsync(lesson);
            return Ok(createdLesson);
        }

        [HttpPut("{id}")]
        [RequireAdmin]
        public async Task<IActionResult> UpdateLesson(Guid id, [FromBody] UpdateLessonDto lesson)
        {
            var updateLesson = await _lessonService.UpdateOneAsync(id, lesson);
            return Ok(updateLesson);
        }

    }
}

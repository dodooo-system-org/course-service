using course_service.Attributes;
using course_service.Modules.Course.DTOs;
using course_service.Modules.Course.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace course_service.Modules.Course.Controllers
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly CourseService _courseService;

        public CourseController(CourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return Ok(courses);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseById(Guid id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            return Ok(course);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto createCourseDto)
        {
            var result = await _courseService.CreateCourseAsync(createCourseDto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseDto updateCourseDto)
        {
            var updatedCourse = await _courseService.UpdateCourseAsync(id, updateCourseDto);
            return Ok(updatedCourse);
        }
    }
}

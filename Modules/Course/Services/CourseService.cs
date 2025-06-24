using course_service.Data;
using course_service.Data.Entities;
using course_service.Modules.Course.DTOs;
using course_service.Modules.Course.Interfaces;
using course_service.Modules.Course.Mappers;
using course_service.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace course_service.Modules.Course.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CourseService> _logger;
    public CourseService(AppDbContext context)
    {
        _context = context;
        _logger = LoggerHelper.GetLogger<CourseService>();
    }

    public async Task<CourseEntity> CreateCourseAsync(CreateCourseDto course)
    {
        try
        {
            CourseEntity newCourse = new()
            {
                CourseName = course.CourseName,
                CourseDescription = course.CourseDescription,
                CourseImageUrl = course.CourseImageUrl,
                Level = course.CourseLevel,
                CategoryId = course.CategoryId,
                Category = null!
            };
            _context.Courses.Add(newCourse);
            await _context.SaveChangesAsync();
            return newCourse;
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Create course failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to create course");
        }
    }

    public async Task<List<CourseDto>> GetAllCoursesAsync()
    {
        try
        {
            var courses = await _context.Courses.Include(c => c.Category).ToListAsync();
            return courses.Select(c => c.MapToDto()).ToList();
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Get all courses failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to get all courses");
        }
    }

    public async Task<CourseDto> GetCourseByIdAsync(Guid courseId)
    {
        try
        {
            if (courseId == Guid.Empty)
            {
                throw new ArgumentException("Course ID cannot be empty", nameof(courseId));
            }
            var course = await _context.Courses.Include(c => c.Category).FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                throw new KeyNotFoundException($"Course not found");
            }
            return course.MapToDto();
        }
        catch (Exception error)
        {
            _logger.LogError(error, $"Get course by ID failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to get course by ID");
        }
    }

    public async Task<CourseDto> UpdateCourseAsync(Guid courseId, UpdateCourseDto course)
    {
        try
        {
            var existingCourse = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
            if (existingCourse == null)
            {
                throw new KeyNotFoundException($"Course with ID {courseId} not found");
            }
            existingCourse.CourseName = course.CourseName;
            existingCourse.CourseDescription = course.CourseDescription;
            existingCourse.CourseImageUrl = course.CourseImageUrl;
            existingCourse.Level = course.CourseLevel;
            existingCourse.CategoryId = course.CategoryId;
            existingCourse.UpdatedAt = DateTime.UtcNow;
            _context.Courses.Update(existingCourse);
            await _context.SaveChangesAsync();
            return existingCourse.MapToDto();
        }
        catch (Exception error)
        {
            _logger.LogError(error, $"Update course by ID failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to update course by ID");
        }
    }
}

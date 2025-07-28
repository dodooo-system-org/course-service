using course_service.Data;
using course_service.Data.Entities;
using course_service.Modules.Caching.Interfaces;
using course_service.Modules.Category.Interfaces;
using course_service.Modules.Course.DTOs;
using course_service.Modules.Course.Interfaces;
using course_service.Modules.Course.Mappers;
using course_service.Shared.DTOs;
using course_service.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace course_service.Modules.Course.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _context;
    private readonly ICategoryService _categoryService;
    private readonly ICourseCachingService _courseCachingService;
    private readonly ILogger<CourseService> _logger;
    public CourseService(AppDbContext context, ICategoryService categoryService, ICourseCachingService courseCachingService)
    {
        _context = context;
        _categoryService = categoryService;
        _courseCachingService = courseCachingService;
        _logger = LoggerHelper.GetLogger<CourseService>();
    }

    public async Task<CourseEntity> CreateCourseAsync(CreateCourseDto course)
    {
        try
        {
            // Check if the category exists
            var category = await _categoryService.GetOneAsync(course.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category not found");
            }

            CourseEntity newCourse = new()
            {
                CourseName = course.CourseName,
                CourseDescription = course.CourseDescription,
                CourseImageUrl = course.CourseImageUrl,
                Level = course.CourseLevel,
                CategoryId = course.CategoryId,
                Category = category
            };
            _context.Courses.Add(newCourse);
            await _context.SaveChangesAsync();

            // clear all courses cache without await for better performance
            _courseCachingService.RemoveAllCoursesFromCacheAsync();
            return newCourse;
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Create course failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to create course");
        }
    }

    public async Task<MetaPaginationDto<List<CourseDto>>> GetAllCoursesAsync(AllCourseQueryDto queryDto)
    {
        try
        {
            string unique = JsonConvert.SerializeObject(queryDto).ToString();
            var cachedCourses = await _courseCachingService.GetAllCoursesFromCacheAsync(unique);
            if (cachedCourses != null)
            {
                return cachedCourses;
            }

            var query = _context.Courses.Include(c => c.Category).AsQueryable();

            // Apply filters based on queryDto
            if (queryDto.CategoryId != null)
            {
                query = query.Where(c => c.CategoryId == queryDto.CategoryId);
            }

            if (queryDto.CourseLevel != null)
            {
                query = query.Where(c => c.Level == queryDto.CourseLevel);
            }

            if (queryDto.IsActive != null)
            {
                query = query.Where(c => c.IsActive == queryDto.IsActive);
            }

            // if the IsDeleted filter is not provided, default to false - just show not deleted courses
            if (queryDto.IsDeleted == true)
            {
                query = query.Where(c => c.IsDeleted == queryDto.IsDeleted);
            }
            else
            {
                query = query.Where(c => c.IsDeleted != true);
            }

            if (!String.IsNullOrEmpty(queryDto.Query))
            {
                // Use raw SQL for full-text search and then join with other filters
                var courseIds = await _context.Courses
                    .FromSqlRaw("SELECT course_id FROM Courses WHERE MATCH(course_name) AGAINST ({0} IN NATURAL LANGUAGE MODE)", queryDto.Query)
                    .Select(c => c.CourseId)
                    .ToListAsync();

                query = query.Where(c => courseIds.Contains(c.CourseId));
            }

            var coursesWithCounts = await query
            .Skip((queryDto.Page - 1) * queryDto.Size)
            .Take(queryDto.Size)
            .Select(c => new
            {
                Course = c,
                ModuleCount = _context.Modules.Count(m => m.CourseId == c.CourseId && !m.IsDeleted),
                LessonCount = _context.Modules
                    .Where(m => m.CourseId == c.CourseId && !m.IsDeleted)
                    .SelectMany(m => _context.Lessons.Where(l => l.ModuleId == m.ModuleId && !l.IsDeleted))
                    .Count()
            })
            .ToListAsync();

            int totalCount = await query.CountAsync();

            var courses = coursesWithCounts.Select(x => x.Course.MapToDto(x.ModuleCount, x.LessonCount)).ToList();


            var result = new MetaPaginationDto<List<CourseDto>>
            {
                Meta = new MetaDto
                {
                    Page = queryDto.Page,
                    Size = queryDto.Size,
                    TotalCount = totalCount
                },
                Data = courses
            };

            // Cache the courses
            await _courseCachingService.CacheAllCoursesAsync(result, unique);

            return result;
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
                throw new ArgumentException("Course ID cannot be empty");
            }
            var course = await _context.Courses.Include(c => c.Category).Select(c => new
            {
                Course = c,
                ModuleCount = _context.Modules.Count(m => m.CourseId == c.CourseId && !m.IsDeleted),
                LessonCount = _context.Modules
                    .Where(m => m.CourseId == c.CourseId && !m.IsDeleted)
                    .SelectMany(m => _context.Lessons.Where(l => l.ModuleId == m.ModuleId && !l.IsDeleted))
                    .Count()
            }).Where(c => c.Course.CourseId == courseId).FirstOrDefaultAsync();
            if (course == null)
            {
                throw new KeyNotFoundException($"Course not found");
            }

            return course.Course.MapToDto(course.ModuleCount, course.LessonCount);
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
            if (courseId == Guid.Empty)
            {
                throw new ArgumentException("Course ID cannot be empty");
            }
            // Validate and check categoryId existence
            var category = await _categoryService.GetOneAsync(course.CategoryId);

            var existingCourse = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
            if (existingCourse == null)
            {
                throw new KeyNotFoundException($"Course not found");
            }
            existingCourse.CourseName = course.CourseName;
            existingCourse.CourseDescription = course.CourseDescription;
            existingCourse.CourseImageUrl = course.CourseImageUrl;
            existingCourse.Level = course.CourseLevel;
            existingCourse.CategoryId = category.CategoryId;
            existingCourse.Category = category;
            existingCourse.UpdatedAt = DateTime.UtcNow;
            _context.Courses.Update(existingCourse);
            await _context.SaveChangesAsync();

            // Get module and lesson counts
            var moduleCount = await _context.Modules.CountAsync(m => m.CourseId == courseId && !m.IsDeleted);
            var lessonCount = await _context.Modules
                .Where(m => m.CourseId == courseId && !m.IsDeleted)
                .SelectMany(m => _context.Lessons.Where(l => l.ModuleId == m.ModuleId && !l.IsDeleted))
                .CountAsync();

            // Clear the cache for this course and list of courses
            _courseCachingService.RemoveCourseFromCacheAsync(courseId);
            _courseCachingService.RemoveAllCoursesFromCacheAsync();

            return existingCourse.MapToDto(moduleCount, lessonCount);
        }
        catch (Exception error)
        {
            _logger.LogError(error, $"Update course by ID failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to update course by ID");
        }
    }
}

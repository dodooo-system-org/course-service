using System;
using course_service.Modules.Course.DTOs;

namespace course_service.Modules.Caching.Interfaces;

public interface ICourseCachingService
{
    Task<List<CourseDto>?> GetAllCoursesFromCacheAsync(string unique);
    Task<bool?> CacheAllCoursesAsync(List<CourseDto> courses, string unique);
    Task<bool?> RemoveAllCoursesFromCacheAsync();

    Task<CourseDto?> GetCourseFromCacheAsync(Guid courseId);
    Task<bool?> CacheCourseAsync(CourseDto courseDto);
    Task<bool?> RemoveCourseFromCacheAsync(Guid courseId);
}

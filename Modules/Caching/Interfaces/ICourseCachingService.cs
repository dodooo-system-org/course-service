using System;
using course_service.Modules.Course.DTOs;
using course_service.Shared.DTOs;

namespace course_service.Modules.Caching.Interfaces;

public interface ICourseCachingService
{
    Task<MetaPaginationDto<List<CourseDto>>?> GetAllCoursesFromCacheAsync(string unique);
    Task<bool?> CacheAllCoursesAsync(MetaPaginationDto<List<CourseDto>> result, string unique);
    Task<bool?> RemoveAllCoursesFromCacheAsync();

    Task<CourseDto?> GetCourseFromCacheAsync(Guid courseId);
    Task<bool?> CacheCourseAsync(CourseDto courseDto);
    Task<bool?> RemoveCourseFromCacheAsync(Guid courseId);

    Task<MetaPaginationDto<List<CourseDto>>?> GetDeletedCoursesFromCacheAsync(string unique);
    Task<bool?> CacheDeletedCoursesAsync(MetaPaginationDto<List<CourseDto>> result, string unique);
    Task<bool?> RemoveDeletedCoursesFromCacheAsync();
}

using System;
using course_service.Modules.Caching.Interfaces;
using course_service.Modules.Course.DTOs;
using course_service.Shared.Helpers;
using course_service.Shared.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace course_service.Modules.Caching.Services;

public class CourseCachingService : ICourseCachingService
{
    private readonly IDistributedCache _cache;
    private readonly ICacheManager _cacheManager;
    private readonly ILogger<CourseCachingService> _logger;

    // keys
    private readonly string _allCoursesCacheKey = "all_courses";
    private readonly string _courseDetailsCacheKey = "course_details";

    public CourseCachingService(IDistributedCache cache, ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
        _cache = cache;
        _logger = LoggerHelper.GetLogger<CourseCachingService>();
    }
    public async Task<bool?> CacheAllCoursesAsync(List<CourseDto> courses, string unique)
    {
        try
        {
            var serializedCourses = JsonConvert.SerializeObject(courses);
            // Cache courses for 1 hour
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _cache.SetAsync($"{_allCoursesCacheKey}:{unique}", System.Text.Encoding.UTF8.GetBytes(serializedCourses), cacheOptions);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache courses");
            return null;
        }
    }

    public async Task<List<CourseDto>?> GetAllCoursesFromCacheAsync(string unique)
    {
        try
        {
            string cacheKey = $"{_allCoursesCacheKey}:{unique}";
            var cachedCourses = await _cache.GetAsync(cacheKey);
            if (cachedCourses == null || cachedCourses.Length == 0)
            {
                return null;
            }

            var cachedCoursesString = System.Text.Encoding.UTF8.GetString(cachedCourses);
            return JsonConvert.DeserializeObject<List<CourseDto>>(cachedCoursesString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve courses from cache");
            return null;
        }
    }

    public async Task<bool?> RemoveAllCoursesFromCacheAsync()
    {
        try
        {
            await _cacheManager.RemoveByPattern($"{_allCoursesCacheKey}:*");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove all courses from cache");
            return null;
        }
    }

    public async Task<CourseDto?> GetCourseFromCacheAsync(Guid courseId)
    {
        try
        {
            var cachedCourse = await _cache.GetAsync($"{_courseDetailsCacheKey}:{courseId}");
            if (cachedCourse == null)
            {
                return null;
            }

            var cachedCourseString = System.Text.Encoding.UTF8.GetString(cachedCourse);
            return JsonConvert.DeserializeObject<CourseDto>(cachedCourseString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve course details from cache");
            return null;
        }
    }

    public async Task<bool?> CacheCourseAsync(CourseDto courseDto)
    {
        try
        {
            var serializedCourse = JsonConvert.SerializeObject(courseDto);
            // Cache course details for 1 hour
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _cache.SetAsync($"{_courseDetailsCacheKey}:{courseDto.CourseId}", System.Text.Encoding.UTF8.GetBytes(serializedCourse), cacheOptions);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache course");
            return null;
        }
    }

    public async Task<bool?> RemoveCourseFromCacheAsync(Guid courseId)
    {
        try
        {
            await _cache.RemoveAsync($"{_courseDetailsCacheKey}:{courseId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove course from cache");
            return null;
        }
    }

}

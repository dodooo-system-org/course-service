using System;
using System.Text;
using course_service.Data.Entities;
using course_service.Modules.Caching.Interfaces;
using course_service.Shared.DTOs;
using course_service.Shared.Helpers;
using course_service.Shared.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace course_service.Modules.Caching.Services;

public class CategoryCachingService : ICategoryCachingService
{
    private readonly IDistributedCache _cache;
    private readonly ICacheManager _cacheManager;
    private readonly string _listAllCategoriesCacheKey = "all_categories";
    private readonly ILogger<CategoryCachingService> _logger;
    public CategoryCachingService(IDistributedCache cache, ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
        _cache = cache;
        _logger = LoggerHelper.GetLogger<CategoryCachingService>();
    }
    public async Task<bool?> CacheListAllCategoriesAsync(MetaPaginationDto<List<CategoryEntity>> result, string unique)
    {
        try
        {
            var serializedCategories = JsonConvert.SerializeObject(result);
            // Cache categories for 1 hour
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _cache.SetAsync($"{_listAllCategoriesCacheKey}:{unique}", Encoding.UTF8.GetBytes(serializedCategories), cacheOptions);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching all categories");
            return null;
        }
    }

    public async Task<bool?> CacheListAvailableCategoriesAsync(List<CategoryEntity> result)
    {
        try
        {
            var serializedCategories = JsonConvert.SerializeObject(result);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _cache.SetAsync($"{_listAllCategoriesCacheKey}:available", Encoding.UTF8.GetBytes(serializedCategories), cacheOptions);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching available categories");
            return null;
        }
    }

    public async Task<MetaPaginationDto<List<CategoryEntity>>?> GetListAllCategoriesAsync(string unique)
    {
        try
        {
            var cachedData = await _cache.GetAsync($"{_listAllCategoriesCacheKey}:{unique}");
            if (cachedData == null || cachedData.Length == 0)
            {
                return null;
            }
            return JsonConvert.DeserializeObject<MetaPaginationDto<List<CategoryEntity>>>(Encoding.UTF8.GetString(cachedData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached all categories");
            return null;
        }
    }

    public async Task<List<CategoryEntity>?> GetListAvailableCategoriesAsync()
    {
        try
        {
            var cachedData = await _cache.GetAsync($"{_listAllCategoriesCacheKey}:available");
            if (cachedData == null || cachedData.Length == 0)
            {
                return null;
            }
            return JsonConvert.DeserializeObject<List<CategoryEntity>>(Encoding.UTF8.GetString(cachedData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached all categories");
            return null;
        }
    }

    public async Task<bool?> RemoveListAllCategoriesAsync()
    {
        try
        {
            await _cacheManager.RemoveByPattern($"{_listAllCategoriesCacheKey}:*");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached all categories");
            return null;
        }
    }
}

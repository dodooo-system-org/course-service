using System;
using course_service.Data.Entities;
using course_service.Shared.DTOs;

namespace course_service.Modules.Caching.Interfaces;

public interface ICategoryCachingService
{
    Task<bool?> CacheListAllCategoriesAsync(MetaPaginationDto<List<CategoryEntity>> result, string unique);
    Task<MetaPaginationDto<List<CategoryEntity>>?> GetListAllCategoriesAsync(string unique);
    Task<bool?> RemoveListAllCategoriesAsync();
    Task<bool?> CacheListAvailableCategoriesAsync(List<CategoryEntity> result);
    Task<List<CategoryEntity>?> GetListAvailableCategoriesAsync();
    Task<bool?> CacheDeletedCategoriesAsync(MetaPaginationDto<List<CategoryEntity>> result, string unique);
    Task<MetaPaginationDto<List<CategoryEntity>>?> GetDeletedCategoriesAsync(string unique);
    Task<bool?> RemoveDeletedCategoriesAsync();
}

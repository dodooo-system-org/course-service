using System;
using course_service.Data;
using course_service.Data.Entities;
using course_service.Modules.Caching.Interfaces;
using course_service.Modules.Category.DTOs;
using course_service.Modules.Category.Interfaces;
using course_service.Shared.DTOs;
using course_service.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace course_service.Modules.Category.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryService> _logger;
    private readonly ICategoryCachingService _categoryCachingService;
    public CategoryService(AppDbContext context, ICategoryCachingService categoryCachingService)
    {
        _context = context;
        _logger = LoggerHelper.GetLogger<CategoryService>();
        _categoryCachingService = categoryCachingService;
    }
    public async Task<CategoryEntity> CreateOneAsync(CreateCategoryDto category)
    {
        try
        {
            var existedCategoryName = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName == category.CategoryName);
            if (existedCategoryName != null)
            {
                throw new ArgumentException("Category already exists");
            }

            var newCategory = new CategoryEntity
            {
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
                CategoryImageUrl = category.CategoryImageUrl,
                Status = CategoryStatus.Active
            };
            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            // Clear cache asynchronously, fire-and-forget
            _ = Task.Run(async () =>
            {
                try
                {
                    await _categoryCachingService.RemoveListAllCategoriesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove category cache asynchronously.");
                }
            });
            return newCategory;
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Create category failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Create category failed");
        }
    }

    public async Task<CategoryEntity> GetOneAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Category ID cannot be empty.");
            }
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category not found");
            }
            return category;
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Get category failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to get category");
        }
    }

    public async Task<MetaPaginationDto<List<CategoryEntity>>> GetListAsync(GetListCategoryDto queries)
    {
        try
        {
            string unique = JsonConvert.SerializeObject(queries).ToString();
            var cachedCategories = await _categoryCachingService.GetListAllCategoriesAsync(unique);
            if (cachedCategories != null)
            {
                return cachedCategories;
            }
            var query = _context.Categories.AsQueryable();

            if (queries.IsDeleted == true)
            {
                query = query.Where(c => c.IsDeleted == true);
            }
            else if (queries.IsActive != null)
            {
                query = query.Where(c => c.IsActive == queries.IsActive && c.IsDeleted != true);
            }
            else
            {
                query = query.Where(c => c.IsDeleted != true);
            }

            if (queries.Query != null)
            {
                query = query.Where(c => EF.Functions.Like(c.CategoryName, $"%{queries.Query}%"));
            }
            var totalCount = await query.CountAsync();
            var categories = await query
                .OrderByDescending(c => c.UpdatedAt)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((queries.Page - 1) * queries.Size)
                .Take(queries.Size)
                .ToListAsync();
            var metaPagination = new MetaPaginationDto<List<CategoryEntity>>
            {
                Data = categories,
                Meta = new MetaDto
                {
                    Page = queries.Page,
                    Size = queries.Size,
                    TotalCount = totalCount,
                }
            };

            // Cache the result
            await _categoryCachingService.CacheListAllCategoriesAsync(metaPagination, unique);
            return metaPagination;
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Get categories list failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Get categories list failed");
        }
    }

    public async Task<CategoryEntity> UpdateOneAsync(Guid id, UpdateCategoryDto category)
    {
        try
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Category ID cannot be empty.");
            }
            var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            // Check if this update involves deletion status change
            bool isDeletionStatusChanged = existingCategory.IsDeleted != category.IsDeleted;
            bool wasDeleted = existingCategory.IsDeleted;

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryDescription = category.CategoryDescription;
            existingCategory.CategoryImageUrl = category.CategoryImageUrl;
            existingCategory.IsActive = category.IsActive;
            existingCategory.IsDeleted = category.IsDeleted;
            existingCategory.UpdatedAt = DateTime.UtcNow;

            // Set DeletedAt timestamp when marking as deleted
            if (category.IsDeleted && !wasDeleted)
            {
                existingCategory.DeletedAt = DateTime.UtcNow;
            }
            else if (!category.IsDeleted && wasDeleted)
            {
                existingCategory.DeletedAt = null;
            }

            await _context.SaveChangesAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    await _categoryCachingService.RemoveListAllCategoriesAsync();

                    if (isDeletionStatusChanged)
                    {
                        await _categoryCachingService.RemoveDeletedCategoriesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove category caches asynchronously.");
                }
            });

            return existingCategory;
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Update category failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to update category");
        }
    }

    public async Task<IEnumerable<CategoryEntity>> GetAvailableCategoriesAsync()
    {
        try
        {
            var cachedAvailableCategories = await _categoryCachingService.GetListAvailableCategoriesAsync();
            if (cachedAvailableCategories != null)
            {
                return cachedAvailableCategories;
            }
            // Fetch from database if not cached
            var availableCategories = await _context.Categories
                .Where(c => c.IsActive == true && c.IsDeleted != true).ToListAsync();

            // Cache the available categories
            await _categoryCachingService.CacheListAvailableCategoriesAsync(availableCategories);

            return availableCategories;
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Get available categories failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to get available categories");
        }
    }

    public async Task<MetaPaginationDto<List<CategoryEntity>>> GetDeletedCategoriesAsync(GetDeletedCategoriesDto dto)
    {
        try
        {
            // Create unique key for caching based on query parameters
            string unique = JsonConvert.SerializeObject(dto);

            // Try to get from cache first
            var cachedDeletedCourses = await _categoryCachingService.GetDeletedCategoriesAsync(unique);
            if (cachedDeletedCourses != null)
            {
                return cachedDeletedCourses;
            }

            // Build query if not in cache
            var query = _context.Categories.AsQueryable();
            if (!string.IsNullOrEmpty(dto.Query))
            {
                query = query.Where(c => EF.Functions.Like(c.CategoryName, $"%{dto.Query}%"));
            }
            if (dto.StartDate.HasValue)
            {
                query = query.Where(c => c.DeletedAt >= dto.StartDate.Value);
            }
            if (dto.EndDate.HasValue)
            {
                query = query.Where(c => c.DeletedAt <= dto.EndDate.Value);
            }

            query = query.Where(c => c.IsDeleted == true);

            int totalCount = await query.CountAsync();

            var deletedCourses = await query
                .Skip((dto.Page - 1) * dto.Size)
                .Take(dto.Size)
                .OrderBy(c => c.DeletedAt)
                .ToListAsync();


            MetaPaginationDto<List<CategoryEntity>> result = new()
            {
                Data = deletedCourses,
                Meta = new()
                {
                    Page = dto.Page,
                    Size = dto.Size,
                    TotalCount = totalCount
                }
            };

            // Cache the result
            await _categoryCachingService.CacheDeletedCategoriesAsync(result, unique);

            return result;
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Get deleted courses failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to get deleted courses");
        }
    }
}

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

            _categoryCachingService.RemoveListAllCategoriesAsync();
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

    public async Task<MetaPaginationDto<List<CategoryEntity>>> GetListAsync(PaginationDto pagination)
    {
        try
        {
            string unique = JsonConvert.SerializeObject(pagination).ToString();
            var cachedCategories = await _categoryCachingService.GetListAllCategoriesAsync(unique);
            if (cachedCategories != null)
            {
                return cachedCategories;
            }
            var query = _context.Categories.AsQueryable();
            if (pagination.Query != null)
            {
                query = query.Where(c => EF.Functions.Like(c.CategoryName, $"%{pagination.Query}%"));
            }
            var totalCount = await query.CountAsync();
            var categories = await query
                .OrderByDescending(c => c.UpdatedAt)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pagination.Page - 1) * pagination.Size)
                .Take(pagination.Size)
                .ToListAsync();
            var metaPagination = new MetaPaginationDto<List<CategoryEntity>>
            {
                Data = categories,
                Meta = new MetaDto
                {
                    Page = pagination.Page,
                    Size = pagination.Size,
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
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException("Category not found");
            }
            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryDescription = category.CategoryDescription;
            existingCategory.CategoryImageUrl = category.CategoryImageUrl;
            existingCategory.Status = category.Status;
            existingCategory.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Clear cache
            _categoryCachingService.RemoveListAllCategoriesAsync();
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
}

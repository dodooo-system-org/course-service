using System;
using course_service.Data;
using course_service.Data.Entities;
using course_service.Modules.Category.DTOs;
using course_service.Modules.Category.Interfaces;
using course_service.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace course_service.Modules.Category.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryService> _logger;
    public CategoryService(AppDbContext context)
    {
        _context = context;
        _logger = LoggerHelper.GetLogger<CategoryService>();
    }
    public async Task<CategoryEntity> CreateOneAsync(CreateCategoryDto category)
    {
        try
        {
            var newCategory = new CategoryEntity
            {
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
                CategoryImageUrl = category.CategoryImageUrl,
                Status = CategoryStatus.Active
            };
            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();
            return newCategory;
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Create category failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to create category");
        }
    }

    public async Task<CategoryEntity> GetOneAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Category ID cannot be empty.", nameof(id));
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

    public async Task<IEnumerable<CategoryEntity>> GetListAsync()
    {
        try
        {
            return await Task.FromResult(_context.Categories.ToList());
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Get categories list failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to get categories list");
        }
    }

    public async Task<CategoryEntity> UpdateOneAsync(Guid id, UpdateCategoryDto category)
    {
        try
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                throw new Exception("Category not found");
            }
            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryDescription = category.CategoryDescription;
            existingCategory.CategoryImageUrl = category.CategoryImageUrl;
            existingCategory.Status = category.Status;
            existingCategory.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingCategory;
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Update category failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to update category");
        }
    }
}

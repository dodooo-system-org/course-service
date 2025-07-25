using course_service.Data;
using course_service.Data.Entities;
using course_service.Modules.Category.DTOs;
using course_service.Modules.Category.Interfaces;
using course_service.Modules.Category.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace course_service.tests;

public class CategoryServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ICategoryService _categoryService;
    public CategoryServiceTests()
    {
        // Disable logging for tests
        Shared.Helpers.LoggerHelper.SetLoggerFactory(new NoOpLoggerFactory());

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "CategoryServiceTestDb")
            .Options;

        _context = new AppDbContext(options);
        _categoryService = new CategoryService(_context);
    }

    // Dispose the context after tests
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }


    #region CreateOneAsync
    [Fact]
    public async Task CreateOneAsync_ValidCategory_ReturnCreatedCategory()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test Category",
            CategoryDescription = "This is a test category.",
            CategoryImageUrl = "http://example.com/image.jpg"
        };

        // Act
        var createdCategory = await _categoryService.CreateOneAsync(createCategoryDto);

        // Assert
        Assert.NotNull(createdCategory);
        Assert.Equal(createCategoryDto.CategoryName, createdCategory.CategoryName);
        Assert.Equal(createCategoryDto.CategoryDescription, createdCategory.CategoryDescription);
        Assert.Equal(createCategoryDto.CategoryImageUrl, createdCategory.CategoryImageUrl);
    }

    [Fact]
    public async Task CreateOneAsync_CategoryAlreadyExists_ThrowsArgumentException()
    {
        // Arrange
        var existingCategoryName = "Existing Category";
        var existingCategory = new CategoryEntity
        {
            CategoryName = existingCategoryName,
            CategoryDescription = "This is an existing category.",
            CategoryImageUrl = "http://example.com/existing.jpg",
            Status = CategoryStatus.Active
        };

        // Add the existing category to the context
        _context.Categories.Add(existingCategory);
        await _context.SaveChangesAsync();

        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = existingCategoryName, // Same name as existing category
            CategoryDescription = "This is a duplicate category.",
            CategoryImageUrl = "http://example.com/duplicate.jpg"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _categoryService.CreateOneAsync(createCategoryDto));

        Assert.Equal("Category already exists", exception.Message);
    }

    [Fact]
    public async Task CreateOneAsync_NullCategoryName_ThrowException()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = null!, // Invalid category name
            CategoryDescription = "This is a test category.",
            CategoryImageUrl = "http://example.com/image.jpg"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _categoryService.CreateOneAsync(createCategoryDto));
        Assert.Equal("Create category failed", exception.Message);
    }

    [Fact]
    public async Task CreateOneAsync_NullCategoryDescription_ThrowException()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test category",
            CategoryDescription = null!, // Invalid category description
            CategoryImageUrl = "http://example.com/image.jpg"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _categoryService.CreateOneAsync(createCategoryDto));
        Assert.Equal("Create category failed", exception.Message);
    }

    [Fact]
    public async Task CreateOneAsync_NullCategoryImageUrl_ThrowException()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test category",
            CategoryDescription = "This is a test category.",
            CategoryImageUrl = null! // Invalid category image URL
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _categoryService.CreateOneAsync(createCategoryDto));
        Assert.Equal("Create category failed", exception.Message);
    }
    #endregion

    #region GetOneAsync
    [Fact]
    public async Task GetOneAsync_ValidId_ReturnsCategory()
    {
        // Arrange
        var category = new CategoryEntity
        {
            CategoryName = "Test Category",
            CategoryDescription = "This is a test category.",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _categoryService.GetOneAsync(category.CategoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.CategoryName, result.CategoryName);
    }

    [Fact]
    public async Task GetOneAsync_InvalidId_ThrowsArgumentException()
    {
        // Arrange
        var invalidId = Guid.Empty; // Invalid ID

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _categoryService.GetOneAsync(invalidId));
        Assert.Equal("Category ID cannot be empty.", exception.Message);
    }

    [Fact]
    public async Task GetOneAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid(); // Non-existent ID

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _categoryService.GetOneAsync(nonExistentId));
        Assert.Equal("Category not found", exception.Message);
    }
    #endregion

    #region GetAllAsync
    [Fact]
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        // Arrange
        var category1 = new CategoryEntity
        {
            CategoryName = "Category 1",
            CategoryDescription = "Description 1",
            CategoryImageUrl = "http://example.com/image1.jpg"
        };
        var category2 = new CategoryEntity
        {
            CategoryName = "Category 2",
            CategoryDescription = "Description 2",
            CategoryImageUrl = "http://example.com/image2.jpg"
        };
        _context.Categories.AddRange(category1, category2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _categoryService.GetListAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.CategoryName == category1.CategoryName);
        Assert.Contains(result, c => c.CategoryName == category2.CategoryName);
    }

    [Fact]
    public async Task GetAllAsync_NoCategories_ReturnsEmptyList()
    {
        // Arrange
        // No categories added to the context

        // Act
        var result = await _categoryService.GetListAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    #endregion

    #region UpdateOneAsync
    [Fact]
    public async Task UpdateOneAsync_ValidCategory_ReturnsUpdatedCategory()
    {
        // Arrange
        var existingCategory = new CategoryEntity
        {
            CategoryName = "Old Category",
            CategoryDescription = "Old Description",
            CategoryImageUrl = "http://example.com/old.jpg"
        };
        _context.Categories.Add(existingCategory);
        await _context.SaveChangesAsync();

        var updateCategoryDto = new UpdateCategoryDto
        {
            CategoryName = "Updated Category",
            CategoryDescription = "Updated Description",
            CategoryImageUrl = "http://example.com/updated.jpg"
        };

        // Act
        var updatedCategory = await _categoryService.UpdateOneAsync(existingCategory.CategoryId, updateCategoryDto);

        // Assert
        Assert.NotNull(updatedCategory);
        Assert.Equal(updateCategoryDto.CategoryName, updatedCategory.CategoryName);
        Assert.Equal(updateCategoryDto.CategoryDescription, updatedCategory.CategoryDescription);
        Assert.Equal(updateCategoryDto.CategoryImageUrl, updatedCategory.CategoryImageUrl);
    }

    [Fact]
    public async Task UpdateOneAsync_InvalidId_ThrowsArgumentException()
    {
        // Arrange
        var invalidId = Guid.Empty; // Invalid ID
        var updateCategoryDto = new UpdateCategoryDto
        {
            CategoryName = "Updated Category",
            CategoryDescription = "Updated Description",
            CategoryImageUrl = "http://example.com/updated.jpg"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _categoryService.UpdateOneAsync(invalidId, updateCategoryDto));
        Assert.Equal("Category ID cannot be empty.", exception.Message);
    }

    [Fact]
    public async Task UpdateOneAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid(); // Non-existent ID
        var updateCategoryDto = new UpdateCategoryDto
        {
            CategoryName = "Updated Category",
            CategoryDescription = "Updated Description",
            CategoryImageUrl = "http://example.com/updated.jpg"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _categoryService.UpdateOneAsync(nonExistentId, updateCategoryDto));
        Assert.Equal("Category not found", exception.Message);
    }
    #endregion
}

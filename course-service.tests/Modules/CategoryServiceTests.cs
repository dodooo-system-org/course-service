using course_service.Data;
using course_service.Data.Entities;
using course_service.Modules.Caching.Interfaces;
using course_service.Modules.Category.DTOs;
using course_service.Modules.Category.Interfaces;
using course_service.Modules.Category.Services;
using course_service.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace course_service.tests;

public class CategoryServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ICategoryService _categoryService;
    private readonly Mock<ICategoryCachingService> _mockCachingService;

    public CategoryServiceTests()
    {
        // Disable logging for tests
        Shared.Helpers.LoggerHelper.SetLoggerFactory(new NoOpLoggerFactory());

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "CategoryServiceTestDb")
            .Options;

        _context = new AppDbContext(options);
        _mockCachingService = new Mock<ICategoryCachingService>();
        _categoryService = new CategoryService(_context, _mockCachingService.Object);
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

        // Note: Cache removal is fire-and-forget with Task.Run, so we can't reliably verify it was called
        // The method returns immediately without waiting for cache removal to complete
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
    public async Task GetListAsync_WithValidPagination_ReturnsAllCategories()
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

        var paginationDto = new GetListCategoryDto
        {
            Page = 1,
            Size = 10,
        };

        // Setup caching service mock
        _mockCachingService.Setup(x => x.GetListAllCategoriesAsync(It.IsAny<string>()))
            .ReturnsAsync((MetaPaginationDto<List<CategoryEntity>>?)null);
        _mockCachingService.Setup(x => x.CacheListAllCategoriesAsync(It.IsAny<MetaPaginationDto<List<CategoryEntity>>>(), It.IsAny<string>()))
            .ReturnsAsync((bool?)true);

        // Act
        var result = await _categoryService.GetListAsync(paginationDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Contains(result.Data, c => c.CategoryName == category1.CategoryName);
        Assert.Contains(result.Data, c => c.CategoryName == category2.CategoryName);
        Assert.Equal(1, result.Meta.Page);
        Assert.Equal(10, result.Meta.Size);
        Assert.Equal(2, result.Meta.TotalCount);
    }

    [Fact]
    public async Task GetListAsync_NoCategories_ReturnsEmptyList()
    {
        // Arrange
        var paginationDto = new GetListCategoryDto
        {
            Page = 1,
            Size = 10
        };

        // Setup caching service mock
        _mockCachingService.Setup(x => x.GetListAllCategoriesAsync(It.IsAny<string>()))
            .ReturnsAsync((MetaPaginationDto<List<CategoryEntity>>?)null);
        _mockCachingService.Setup(x => x.CacheListAllCategoriesAsync(It.IsAny<MetaPaginationDto<List<CategoryEntity>>>(), It.IsAny<string>()))
            .ReturnsAsync((bool?)true);

        // Act
        var result = await _categoryService.GetListAsync(paginationDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.Equal(1, result.Meta.Page);
        Assert.Equal(10, result.Meta.Size);
        Assert.Equal(0, result.Meta.TotalCount);
    }

    [Fact]
    public async Task GetListAsync_WithIsActiveFilter_ReturnsFilteredCategories()
    {
        // Arrange
        var activeCategory = new CategoryEntity
        {
            CategoryName = "Active Category",
            CategoryDescription = "This is an active category.",
            CategoryImageUrl = "http://example.com/active.jpg",
            IsActive = true,
            IsDeleted = false
        };

        var inactiveCategory = new CategoryEntity
        {
            CategoryName = "Inactive Category",
            CategoryDescription = "This is an inactive category.",
            CategoryImageUrl = "http://example.com/inactive.jpg",
            IsActive = false,
            IsDeleted = false
        };

        _context.Categories.AddRange(activeCategory, inactiveCategory);
        await _context.SaveChangesAsync();

        var paginationDto = new GetListCategoryDto
        {
            Page = 1,
            Size = 10,
            IsActive = true
        };

        // Setup caching service mock
        _mockCachingService.Setup(x => x.CacheListAllCategoriesAsync(It.IsAny<MetaPaginationDto<List<CategoryEntity>>>(), It.IsAny<string>()))
            .ReturnsAsync((bool?)true);

        // Act
        var result = await _categoryService.GetListAsync(paginationDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal(activeCategory.CategoryName, result.Data.First().CategoryName);
        Assert.Equal(1, result.Meta.TotalCount);
    }

    [Fact]
    public async Task GetListAsync_WithIsDeletedFilter_ReturnsDeletedCategories()
    {
        // Arrange
        var activeCategory = new CategoryEntity
        {
            CategoryName = "Active Category",
            CategoryDescription = "This is an active category.",
            CategoryImageUrl = "http://example.com/active.jpg",
            IsActive = true,
            IsDeleted = false
        };

        var deletedCategory = new CategoryEntity
        {
            CategoryName = "Deleted Category",
            CategoryDescription = "This is a deleted category.",
            CategoryImageUrl = "http://example.com/deleted.jpg",
            IsActive = false,
            IsDeleted = true
        };

        _context.Categories.AddRange(activeCategory, deletedCategory);
        await _context.SaveChangesAsync();

        var paginationDto = new GetListCategoryDto
        {
            Page = 1,
            Size = 10,
            IsDeleted = true
        };

        // Setup caching service mock
        _mockCachingService.Setup(x => x.CacheListAllCategoriesAsync(It.IsAny<MetaPaginationDto<List<CategoryEntity>>>(), It.IsAny<string>()))
            .ReturnsAsync((bool?)true);

        // Act
        var result = await _categoryService.GetListAsync(paginationDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal(deletedCategory.CategoryName, result.Data.First().CategoryName);
        Assert.Equal(1, result.Meta.TotalCount);
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

        // Note: Cache removal is fire-and-forget with Task.Run, so we can't reliably verify it was called
        // The method returns immediately without waiting for cache removal to complete
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

    #region GetAvailableCategoriesAsync
    [Fact]
    public async Task GetAvailableCategoriesAsync_WithActiveCategories_ReturnsActiveCategoriesOnly()
    {
        // Arrange
        var activeCategory1 = new CategoryEntity
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Active Category 1",
            CategoryDescription = "This is an active category.",
            CategoryImageUrl = "http://example.com/active1.jpg",
            Status = CategoryStatus.Active,
            IsActive = true
        };

        var activeCategory2 = new CategoryEntity
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Active Category 2",
            CategoryDescription = "This is another active category.",
            CategoryImageUrl = "http://example.com/active2.jpg",
            Status = CategoryStatus.Active,
            IsActive = true
        };

        var inactiveCategory = new CategoryEntity
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Inactive Category",
            CategoryDescription = "This is an inactive category.",
            CategoryImageUrl = "http://example.com/inactive.jpg",
            Status = CategoryStatus.Active,
            IsActive = false
        };

        _context.Categories.AddRange(activeCategory1, activeCategory2, inactiveCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _categoryService.GetAvailableCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, category => Assert.True(category.IsActive));
        Assert.Contains(resultList, c => c.CategoryId == activeCategory1.CategoryId);
        Assert.Contains(resultList, c => c.CategoryId == activeCategory2.CategoryId);
        Assert.DoesNotContain(resultList, c => c.CategoryId == inactiveCategory.CategoryId);
    }

    [Fact]
    public async Task GetAvailableCategoriesAsync_WithNoActiveCategories_ReturnsEmptyList()
    {
        // Arrange
        var inactiveCategory1 = new CategoryEntity
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Inactive Category 1",
            CategoryDescription = "This is an inactive category.",
            CategoryImageUrl = "http://example.com/inactive1.jpg",
            Status = CategoryStatus.Suspending,
            IsActive = false
        };

        var inactiveCategory2 = new CategoryEntity
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Inactive Category 2",
            CategoryDescription = "This is another inactive category.",
            CategoryImageUrl = "http://example.com/inactive2.jpg",
            Status = CategoryStatus.Active,
            IsActive = false
        };

        _context.Categories.AddRange(inactiveCategory1, inactiveCategory2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _categoryService.GetAvailableCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Empty(resultList);
    }

    [Fact]
    public async Task GetAvailableCategoriesAsync_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        // No categories added to the database

        // Act
        var result = await _categoryService.GetAvailableCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Empty(resultList);
    }
    #endregion
}

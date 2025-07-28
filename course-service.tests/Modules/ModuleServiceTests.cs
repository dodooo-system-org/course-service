using System;
using course_service.Data;
using course_service.Data.Entities;
using course_service.Modules.Caching.Interfaces;
using course_service.Modules.Caching.Services;
using course_service.Modules.Category.DTOs;
using course_service.Modules.Category.Interfaces;
using course_service.Modules.Category.Services;
using course_service.Modules.Course.DTOs;
using course_service.Modules.Course.Interfaces;
using course_service.Modules.Course.Services;
using course_service.Modules.Modules.DTOs;
using course_service.Modules.Modules.Interfaces;
using course_service.Modules.Modules.Services;
using course_service.Shared.Helpers;
using course_service.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace course_service.tests.Modules;

public class ModuleServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IModuleService _moduleService;
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private readonly ICourseCachingService _courseCachingService;

    public ModuleServiceTests()
    {
        // Configure test logger to avoid logging issues
        LoggerHelper.SetLoggerFactory(new NoOpLoggerFactory());

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        var mockCategoryCaching = new Mock<ICategoryCachingService>();
        _categoryService = new CategoryService(_context, mockCategoryCaching.Object);
        _courseCachingService = new CourseCachingService(new Mock<IDistributedCache>().Object, new Mock<ICacheManager>().Object);
        _courseService = new CourseService(_context, _categoryService, _courseCachingService);
        _moduleService = new ModuleService(_context, _courseService);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    #region CreateOneAsync Tests
    [Fact]
    public async Task CreateOneAsync_CreateSuccessfully_ReturnsModuleDto()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        var createdCategory = await _categoryService.CreateOneAsync(createCategoryDto);

        var createCourseDto = new CreateCourseDto
        {
            CourseName = "Test Course",
            CourseDescription = "Test Description",
            CourseImageUrl = "http://example.com/course.jpg",
            CourseLevel = CourseLevel.Beginner,
            CategoryId = createdCategory.CategoryId
        };
        var createdCourse = await _courseService.CreateCourseAsync(createCourseDto);

        var createModuleDto = new CreateModuleDto
        {
            ModuleName = "Test Module",
            ModuleDescription = "Test module description for testing purposes",
            Order = 1,
            CourseId = createdCourse.CourseId
        };

        // Act
        var result = await _moduleService.CreateOneAsync(createModuleDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createModuleDto.ModuleName, result.ModuleName);
        Assert.Equal(createModuleDto.ModuleDescription, result.ModuleDescription);
        Assert.Equal(createModuleDto.Order, result.Order);
        Assert.Equal(createModuleDto.CourseId, result.CourseId);
        Assert.NotEqual(Guid.Empty, result.ModuleId);
    }

    [Fact]
    public async Task CreateOneAsync_CourseNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var createModuleDto = new CreateModuleDto
        {
            ModuleName = "Test Module",
            ModuleDescription = "Test module description for testing purposes",
            Order = 1,
            CourseId = Guid.NewGuid() // Non-existent course ID
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _moduleService.CreateOneAsync(createModuleDto));
        Assert.Equal("Course not found", exception.Message);
    }

    [Fact]
    public async Task CreateOneAsync_ModuleOrderAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        var createdCategory = await _categoryService.CreateOneAsync(createCategoryDto);

        var createCourseDto = new CreateCourseDto
        {
            CourseName = "Test Course",
            CourseDescription = "Test Description",
            CourseImageUrl = "http://example.com/course.jpg",
            CourseLevel = CourseLevel.Beginner,
            CategoryId = createdCategory.CategoryId
        };
        var createdCourse = await _courseService.CreateCourseAsync(createCourseDto);

        // Create first module with order 1
        var firstModuleDto = new CreateModuleDto
        {
            ModuleName = "First Module",
            ModuleDescription = "First module description for testing purposes",
            Order = 1,
            CourseId = createdCourse.CourseId
        };
        await _moduleService.CreateOneAsync(firstModuleDto);

        // Try to create second module with the same order
        var secondModuleDto = new CreateModuleDto
        {
            ModuleName = "Second Module",
            ModuleDescription = "Second module description for testing purposes",
            Order = 1, // Same order as first module
            CourseId = createdCourse.CourseId
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _moduleService.CreateOneAsync(secondModuleDto));
        Assert.Equal("Module order 1 already exists", exception.Message);
    }

    [Fact]
    public async Task CreateOneAsync_DecimalOrderValue_CreatesSuccessfully()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        var createdCategory = await _categoryService.CreateOneAsync(createCategoryDto);

        var createCourseDto = new CreateCourseDto
        {
            CourseName = "Test Course",
            CourseDescription = "Test Description",
            CourseImageUrl = "http://example.com/course.jpg",
            CourseLevel = CourseLevel.Beginner,
            CategoryId = createdCategory.CategoryId
        };
        var createdCourse = await _courseService.CreateCourseAsync(createCourseDto);

        var createModuleDto = new CreateModuleDto
        {
            ModuleName = "Test Module",
            ModuleDescription = "Test module description for testing purposes",
            Order = 1.5m, // Decimal order value
            CourseId = createdCourse.CourseId
        };

        // Act
        var result = await _moduleService.CreateOneAsync(createModuleDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.5m, result.Order);
    }

    [Fact]
    public async Task CreateOneAsync_MultipleModulesWithDifferentOrders_CreatesSuccessfully()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        var createdCategory = await _categoryService.CreateOneAsync(createCategoryDto);

        var createCourseDto = new CreateCourseDto
        {
            CourseName = "Test Course",
            CourseDescription = "Test Description",
            CourseImageUrl = "http://example.com/course.jpg",
            CourseLevel = CourseLevel.Beginner,
            CategoryId = createdCategory.CategoryId
        };
        var createdCourse = await _courseService.CreateCourseAsync(createCourseDto);

        var firstModuleDto = new CreateModuleDto
        {
            ModuleName = "First Module",
            ModuleDescription = "First module description for testing purposes",
            Order = 1,
            CourseId = createdCourse.CourseId
        };

        var secondModuleDto = new CreateModuleDto
        {
            ModuleName = "Second Module",
            ModuleDescription = "Second module description for testing purposes",
            Order = 2, // Different order
            CourseId = createdCourse.CourseId
        };

        var thirdModuleDto = new CreateModuleDto
        {
            ModuleName = "Third Module",
            ModuleDescription = "Third module description for testing purposes",
            Order = 3, // Different order
            CourseId = createdCourse.CourseId
        };

        // Act
        var firstResult = await _moduleService.CreateOneAsync(firstModuleDto);
        var secondResult = await _moduleService.CreateOneAsync(secondModuleDto);
        var thirdResult = await _moduleService.CreateOneAsync(thirdModuleDto);

        // Assert
        Assert.NotNull(firstResult);
        Assert.NotNull(secondResult);
        Assert.NotNull(thirdResult);
        Assert.Equal(1, firstResult.Order);
        Assert.Equal(2, secondResult.Order);
        Assert.Equal(3, thirdResult.Order);
        Assert.All(new[] { firstResult, secondResult, thirdResult },
            result => Assert.Equal(createdCourse.CourseId, result.CourseId));
    }

    [Fact]
    public async Task CreateOneAsync_SameOrderInDifferentCourses_CreatesSuccessfully()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        var createdCategory = await _categoryService.CreateOneAsync(createCategoryDto);

        // Create first course
        var firstCourseDto = new CreateCourseDto
        {
            CourseName = "First Course",
            CourseDescription = "First course description",
            CourseImageUrl = "http://example.com/course1.jpg",
            CourseLevel = CourseLevel.Beginner,
            CategoryId = createdCategory.CategoryId
        };
        var firstCourse = await _courseService.CreateCourseAsync(firstCourseDto);

        // Create second course
        var secondCourseDto = new CreateCourseDto
        {
            CourseName = "Second Course",
            CourseDescription = "Second course description",
            CourseImageUrl = "http://example.com/course2.jpg",
            CourseLevel = CourseLevel.Intermediate,
            CategoryId = createdCategory.CategoryId
        };
        var secondCourse = await _courseService.CreateCourseAsync(secondCourseDto);

        // Create modules with same order in different courses
        var firstModuleDto = new CreateModuleDto
        {
            ModuleName = "Module in First Course",
            ModuleDescription = "Module description for testing purposes",
            Order = 1,
            CourseId = firstCourse.CourseId
        };

        var secondModuleDto = new CreateModuleDto
        {
            ModuleName = "Module in Second Course",
            ModuleDescription = "Module description for testing purposes",
            Order = 1, // Same order but different course
            CourseId = secondCourse.CourseId
        };

        // Act
        var firstResult = await _moduleService.CreateOneAsync(firstModuleDto);
        var secondResult = await _moduleService.CreateOneAsync(secondModuleDto);

        // Assert
        Assert.NotNull(firstResult);
        Assert.NotNull(secondResult);
        Assert.Equal(1, firstResult.Order);
        Assert.Equal(1, secondResult.Order);
        Assert.Equal(firstCourse.CourseId, firstResult.CourseId);
        Assert.Equal(secondCourse.CourseId, secondResult.CourseId);
        Assert.NotEqual(firstResult.ModuleId, secondResult.ModuleId);
    }
    #endregion

    #region GetListAsync Tests
    [Fact]
    public async Task GetListAsync_Success_ReturnsOrderedModules()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        var createdCategory = await _categoryService.CreateOneAsync(createCategoryDto);

        var createCourseDto = new CreateCourseDto
        {
            CourseName = "Test Course",
            CourseDescription = "Test Description",
            CourseImageUrl = "http://example.com/course.jpg",
            CourseLevel = CourseLevel.Beginner,
            CategoryId = createdCategory.CategoryId
        };
        var createdCourse = await _courseService.CreateCourseAsync(createCourseDto);

        // Create multiple modules with different orders
        var firstModuleDto = new CreateModuleDto
        {
            ModuleName = "Third Module",
            ModuleDescription = "Third module description for testing purposes",
            Order = 3,
            CourseId = createdCourse.CourseId
        };

        var secondModuleDto = new CreateModuleDto
        {
            ModuleName = "First Module",
            ModuleDescription = "First module description for testing purposes",
            Order = 1,
            CourseId = createdCourse.CourseId
        };

        var thirdModuleDto = new CreateModuleDto
        {
            ModuleName = "Second Module",
            ModuleDescription = "Second module description for testing purposes",
            Order = 2,
            CourseId = createdCourse.CourseId
        };

        await _moduleService.CreateOneAsync(firstModuleDto);
        await _moduleService.CreateOneAsync(secondModuleDto);
        await _moduleService.CreateOneAsync(thirdModuleDto);

        // Act
        var result = await _moduleService.GetListAsync(createdCourse.CourseId);

        // Assert
        Assert.NotNull(result);
        var modulesList = result.ToList();
        Assert.Equal(3, modulesList.Count);

        // Verify they are ordered by Order property
        Assert.Equal("First Module", modulesList[0].ModuleName);
        Assert.Equal(1, modulesList[0].Order);
        Assert.Equal("Second Module", modulesList[1].ModuleName);
        Assert.Equal(2, modulesList[1].Order);
        Assert.Equal("Third Module", modulesList[2].ModuleName);
        Assert.Equal(3, modulesList[2].Order);

        // Verify all modules belong to the correct course
        Assert.All(modulesList, module => Assert.Equal(createdCourse.CourseId, module.CourseId));
    }

    [Fact]
    public async Task GetListAsync_EmptyResults_ReturnsEmptyCollection()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        var createdCategory = await _categoryService.CreateOneAsync(createCategoryDto);

        var createCourseDto = new CreateCourseDto
        {
            CourseName = "Test Course",
            CourseDescription = "Test Description",
            CourseImageUrl = "http://example.com/course.jpg",
            CourseLevel = CourseLevel.Beginner,
            CategoryId = createdCategory.CategoryId
        };
        var createdCourse = await _courseService.CreateCourseAsync(createCourseDto);

        // Ensure no modules are created for this course

        // Act
        var result = await _moduleService.GetListAsync(createdCourse.CourseId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetListAsync_EmptyCourseId_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyCourseId = Guid.Empty;

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _moduleService.GetListAsync(emptyCourseId));

        Assert.Equal("Course ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task GetListAsync_NonExistentCourseId_ReturnsEmptyCollection()
    {
        // Arrange
        var nonExistentCourseId = Guid.NewGuid();

        // Act
        var result = await _moduleService.GetListAsync(nonExistentCourseId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetOneAsync Tests
    [Fact]
    public async Task GetOneAsync_Success_ReturnsModuleDto()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        var createdCategory = await _categoryService.CreateOneAsync(createCategoryDto);

        var createCourseDto = new CreateCourseDto
        {
            CourseName = "Test Course",
            CourseDescription = "Test Description",
            CourseImageUrl = "http://example.com/course.jpg",
            CourseLevel = CourseLevel.Beginner,
            CategoryId = createdCategory.CategoryId
        };
        var createdCourse = await _courseService.CreateCourseAsync(createCourseDto);

        var createModuleDto = new CreateModuleDto
        {
            ModuleName = "Test Module",
            ModuleDescription = "Test module description for testing purposes",
            Order = 1,
            CourseId = createdCourse.CourseId
        };
        var createdModule = await _moduleService.CreateOneAsync(createModuleDto);

        // Act
        var result = await _moduleService.GetOneAsync(createdModule.ModuleId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdModule.ModuleId, result.ModuleId);
        Assert.Equal(createdModule.ModuleName, result.ModuleName);
        Assert.Equal(createdModule.ModuleDescription, result.ModuleDescription);
        Assert.Equal(createdModule.Order, result.Order);
        Assert.Equal(createdModule.CourseId, result.CourseId);
    }

    [Fact]
    public async Task GetOneAsync_EmptyId_ThrowsArgumentException()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _moduleService.GetOneAsync(emptyId));
        Assert.Equal("Module ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task GetOneAsync_ModuleNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _moduleService.GetOneAsync(nonExistentId));
        Assert.Equal("Module not found", exception.Message);
    }
    #endregion

    #region UpdateOneAsync Tests
    [Fact]
    public async Task UpdateOneAsync_Success_ReturnsUpdatedModuleDto()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        var createdCategory = await _categoryService.CreateOneAsync(createCategoryDto);

        var createCourseDto = new CreateCourseDto
        {
            CourseName = "Test Course",
            CourseDescription = "Test Description",
            CourseImageUrl = "http://example.com/course.jpg",
            CourseLevel = CourseLevel.Beginner,
            CategoryId = createdCategory.CategoryId
        };
        var createdCourse = await _courseService.CreateCourseAsync(createCourseDto);

        var createModuleDto = new CreateModuleDto
        {
            ModuleName = "Original Module",
            ModuleDescription = "Original module description for testing purposes",
            Order = 1,
            CourseId = createdCourse.CourseId
        };
        var createdModule = await _moduleService.CreateOneAsync(createModuleDto);

        var updateModuleDto = new UpdateModuleDto
        {
            ModuleName = "Updated Module",
            ModuleDescription = "Updated module description for testing purposes",
            Order = 2,
            CourseId = createdCourse.CourseId
        };

        // Act
        var result = await _moduleService.UpdateOneAsync(createdModule.ModuleId, updateModuleDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdModule.ModuleId, result.ModuleId);
        Assert.Equal(updateModuleDto.ModuleName, result.ModuleName);
        Assert.Equal(updateModuleDto.ModuleDescription, result.ModuleDescription);
        Assert.Equal(updateModuleDto.Order, result.Order);

        // Verify the module was actually updated in the database
        var updatedModuleFromDb = await _moduleService.GetOneAsync(createdModule.ModuleId);
        Assert.Equal(updateModuleDto.ModuleName, updatedModuleFromDb.ModuleName);
        Assert.Equal(updateModuleDto.ModuleDescription, updatedModuleFromDb.ModuleDescription);
        Assert.Equal(updateModuleDto.Order, updatedModuleFromDb.Order);
    }

    [Fact]
    public async Task UpdateOneAsync_EmptyId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var updateModuleDto = new UpdateModuleDto
        {
            ModuleName = "Updated Module",
            ModuleDescription = "Updated module description for testing purposes",
            Order = 1,
            CourseId = Guid.NewGuid()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _moduleService.UpdateOneAsync(emptyId, updateModuleDto));
        Assert.Equal("Module ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task UpdateOneAsync_ModuleNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateModuleDto = new UpdateModuleDto
        {
            ModuleName = "Updated Module",
            ModuleDescription = "Updated module description for testing purposes",
            Order = 1,
            CourseId = Guid.NewGuid()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _moduleService.UpdateOneAsync(nonExistentId, updateModuleDto));
        Assert.Equal("Module not found", exception.Message);
    }

    [Fact]
    public async Task UpdateOneAsync_PartialUpdate_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        var createdCategory = await _categoryService.CreateOneAsync(createCategoryDto);

        var createCourseDto = new CreateCourseDto
        {
            CourseName = "Test Course",
            CourseDescription = "Test Description",
            CourseImageUrl = "http://example.com/course.jpg",
            CourseLevel = CourseLevel.Beginner,
            CategoryId = createdCategory.CategoryId
        };
        var createdCourse = await _courseService.CreateCourseAsync(createCourseDto);

        var createModuleDto = new CreateModuleDto
        {
            ModuleName = "Original Module",
            ModuleDescription = "Original module description",
            Order = 1,
            CourseId = createdCourse.CourseId
        };
        var createdModule = await _moduleService.CreateOneAsync(createModuleDto);

        // Only update the module name, keep other fields the same
        var updateModuleDto = new UpdateModuleDto
        {
            ModuleName = "Updated Name Only",
            ModuleDescription = "Original module description", // Keep original
            Order = 1, // Keep original
            CourseId = createdCourse.CourseId // Keep original
        };

        // Act
        var result = await _moduleService.UpdateOneAsync(createdModule.ModuleId, updateModuleDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name Only", result.ModuleName);
        Assert.Equal("Original module description", result.ModuleDescription);
        Assert.Equal(1, result.Order);
        Assert.Equal(createdCourse.CourseId, result.CourseId);
    }
    #endregion

}

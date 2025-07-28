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
using course_service.Modules.Lesson;
using course_service.Modules.Lesson.DTOs;
using course_service.Modules.Lesson.Interfaces;
using course_service.Modules.LessonPart.DTOs;
using course_service.Modules.LessonPart.Interfaces;
using course_service.Modules.LessonPart.Services;
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

public class LessonPartServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ILessonPartService _lessonPartService;
    private readonly ILessonService _lessonService;
    private readonly IModuleService _moduleService;
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private readonly ICourseCachingService _courseCachingService;


    public LessonPartServiceTests()
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
        _lessonService = new LessonService(_context, _moduleService);
        _lessonPartService = new LessonPartService(_context, _lessonService);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    #region CreateOneAsync Tests
    [Fact]
    public async Task CreateOneAsync_CreateSuccessfully_ReturnsLessonPartDto()
    {
        // Arrange
        var lesson = await CreateTestLessonAsync();

        var createLessonPartDto = new CreateLessonPartDto
        {
            LessonPartName = "Test Lesson Part",
            LessonPartContent = "Test lesson part content for testing purposes",
            Order = 1,
            LessonVideoUrl = "http://example.com/video.mp4",
            LessonId = lesson.LessonId
        };

        // Act
        var result = await _lessonPartService.CreateOneAsync(createLessonPartDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createLessonPartDto.LessonPartName, result.LessonPartName);
        Assert.Equal(createLessonPartDto.LessonPartContent, result.LessonPartContent);
        Assert.Equal(createLessonPartDto.Order, result.Order);
        Assert.Equal(createLessonPartDto.LessonVideoUrl, result.LessonVideoUrl);
        Assert.Equal(createLessonPartDto.LessonId, result.LessonId);
    }

    [Fact]
    public async Task CreateOneAsync_LessonNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var createLessonPartDto = new CreateLessonPartDto
        {
            LessonPartName = "Test Lesson Part",
            LessonPartContent = "Test lesson part content for testing purposes",
            Order = 1,
            LessonVideoUrl = "http://example.com/video.mp4",
            LessonId = Guid.NewGuid() // Non-existent lesson ID
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _lessonPartService.CreateOneAsync(createLessonPartDto));

        Assert.Contains("Lesson not found", exception.Message);
    }

    [Fact]
    public async Task CreateOneAsync_DuplicateOrder_ThrowsArgumentException()
    {
        // Arrange
        var lesson = await CreateTestLessonAsync();

        // Create first lesson part with order 1
        var firstLessonPartDto = new CreateLessonPartDto
        {
            LessonPartName = "First Lesson Part",
            LessonPartContent = "First lesson part content for testing purposes",
            Order = 1,
            LessonVideoUrl = "http://example.com/video1.mp4",
            LessonId = lesson.LessonId
        };
        await _lessonPartService.CreateOneAsync(firstLessonPartDto);

        // Create second lesson part with same order 1
        var duplicateOrderLessonPartDto = new CreateLessonPartDto
        {
            LessonPartName = "Second Lesson Part",
            LessonPartContent = "Second lesson part content for testing purposes",
            Order = 1, // Same order as first lesson part
            LessonVideoUrl = "http://example.com/video2.mp4",
            LessonId = lesson.LessonId
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _lessonPartService.CreateOneAsync(duplicateOrderLessonPartDto));

        Assert.Contains("Order was existed", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests
    [Fact]
    public async Task GetAllAsync_SuccessfullyReturnsLessonPartsForLesson_ReturnsListOfLessonParts()
    {
        // Arrange
        var lesson = await CreateTestLessonAsync();

        // Create multiple lesson parts for the lesson
        var firstLessonPartDto = new CreateLessonPartDto
        {
            LessonPartName = "First Lesson Part",
            LessonPartContent = "First lesson part content for testing purposes",
            Order = 2,
            LessonVideoUrl = "http://example.com/video1.mp4",
            LessonId = lesson.LessonId
        };
        await _lessonPartService.CreateOneAsync(firstLessonPartDto);

        var secondLessonPartDto = new CreateLessonPartDto
        {
            LessonPartName = "Second Lesson Part",
            LessonPartContent = "Second lesson part content for testing purposes",
            Order = 1,
            LessonVideoUrl = "http://example.com/video2.mp4",
            LessonId = lesson.LessonId
        };
        await _lessonPartService.CreateOneAsync(secondLessonPartDto);

        // Act
        var result = await _lessonPartService.GetAllAsync(lesson.LessonId);

        // Assert
        Assert.NotNull(result);
        var lessonPartsList = result.ToList();
        Assert.Equal(2, lessonPartsList.Count);

        // Check if results are ordered by Order property
        Assert.Equal(1, lessonPartsList[0].Order);
        Assert.Equal(2, lessonPartsList[1].Order);
        Assert.Equal(secondLessonPartDto.LessonPartName, lessonPartsList[0].LessonPartName);
        Assert.Equal(firstLessonPartDto.LessonPartName, lessonPartsList[1].LessonPartName);
        Assert.All(lessonPartsList, lp => Assert.Equal(lesson.LessonId, lp.LessonId));
    }

    [Fact]
    public async Task GetAllAsync_LessonNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentLessonId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _lessonPartService.GetAllAsync(nonExistentLessonId));

        Assert.Contains("Lesson not found", exception.Message);
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ReturnsEmptyList()
    {
        // Arrange
        var lesson = await CreateTestLessonAsync();

        // Act (no lesson parts created for this lesson)
        var result = await _lessonPartService.GetAllAsync(lesson.LessonId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    #endregion

    #region GetOneAsync Tests
    [Fact]
    public async Task GetOneAsync_SuccessfullyReturnsLessonPart_ReturnsLessonPartDto()
    {
        // Arrange
        var lesson = await CreateTestLessonAsync();

        var createLessonPartDto = new CreateLessonPartDto
        {
            LessonPartName = "Test Lesson Part",
            LessonPartContent = "Test lesson part content for testing purposes",
            Order = 1,
            LessonVideoUrl = "http://example.com/video.mp4",
            LessonId = lesson.LessonId
        };
        var createdLessonPart = await _lessonPartService.CreateOneAsync(createLessonPartDto);

        // Act
        var result = await _lessonPartService.GetOneAsync(createdLessonPart.LessonPartId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdLessonPart.LessonPartId, result.LessonPartId);
        Assert.Equal(createLessonPartDto.LessonPartName, result.LessonPartName);
        Assert.Equal(createLessonPartDto.LessonPartContent, result.LessonPartContent);
        Assert.Equal(createLessonPartDto.Order, result.Order);
        Assert.Equal(createLessonPartDto.LessonVideoUrl, result.LessonVideoUrl);
        Assert.Equal(createLessonPartDto.LessonId, result.LessonId);
    }

    [Fact]
    public async Task GetOneAsync_LessonPartNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentLessonPartId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _lessonPartService.GetOneAsync(nonExistentLessonPartId));

        Assert.Contains("Lesson part not found", exception.Message);
    }

    [Fact]
    public async Task GetOneAsync_EmptyId_ThrowsArgumentException()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _lessonPartService.GetOneAsync(emptyId));

        Assert.Contains("Lesson part ID cannot be empty", exception.Message);
    }
    #endregion

    #region UpdateOneAsync Tests
    [Fact]
    public async Task UpdateOneAsync_SuccessfulUpdate_ReturnsUpdatedLessonPartDto()
    {
        // Arrange
        var lesson = await CreateTestLessonAsync();

        var createLessonPartDto = new CreateLessonPartDto
        {
            LessonPartName = "Original Lesson Part",
            LessonPartContent = "Original lesson part content for testing purposes",
            Order = 1,
            LessonVideoUrl = "http://example.com/original.mp4",
            LessonId = lesson.LessonId
        };
        var createdLessonPart = await _lessonPartService.CreateOneAsync(createLessonPartDto);

        var updateLessonPartDto = new UpdateLessonPartDto
        {
            LessonPartName = "Updated Lesson Part",
            LessonPartContent = "Updated lesson part content for testing purposes",
            Order = 2,
            LessonVideoUrl = "http://example.com/updated.mp4",
            LessonId = lesson.LessonId
        };

        // Act
        var result = await _lessonPartService.UpdateOneAsync(createdLessonPart.LessonPartId, updateLessonPartDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdLessonPart.LessonPartId, result.LessonPartId);
        Assert.Equal(updateLessonPartDto.LessonPartName, result.LessonPartName);
        Assert.Equal(updateLessonPartDto.LessonPartContent, result.LessonPartContent);
        Assert.Equal(updateLessonPartDto.Order, result.Order);
        Assert.Equal(updateLessonPartDto.LessonVideoUrl, result.LessonVideoUrl);
    }

    [Fact]
    public async Task UpdateOneAsync_LessonPartNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentLessonPartId = Guid.NewGuid();
        var updateLessonPartDto = new UpdateLessonPartDto
        {
            LessonPartName = "Updated Lesson Part",
            LessonPartContent = "Updated lesson part content for testing purposes",
            Order = 1,
            LessonVideoUrl = "http://example.com/video.mp4",
            LessonId = Guid.NewGuid()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _lessonPartService.UpdateOneAsync(nonExistentLessonPartId, updateLessonPartDto));

        Assert.Contains("Lesson part not found", exception.Message);
    }

    [Fact]
    public async Task UpdateOneAsync_EmptyLessonPartId_ThrowsArgumentException()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var updateLessonPartDto = new UpdateLessonPartDto
        {
            LessonPartName = "Updated Lesson Part",
            LessonPartContent = "Updated lesson part content for testing purposes",
            Order = 1,
            LessonVideoUrl = "http://example.com/video.mp4",
            LessonId = Guid.NewGuid()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _lessonPartService.UpdateOneAsync(emptyId, updateLessonPartDto));

        Assert.Contains("Lesson part ID cannot be empty", exception.Message);
    }
    #endregion

    #region Helper Methods
    private async Task<LessonDto> CreateTestLessonAsync()
    {
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

        var createLessonDto = new CreateLessonDto
        {
            LessonName = "Test Lesson",
            LessonDescription = "Test lesson description for testing purposes",
            Order = 1,
            Duration = 30,
            ModuleId = createdModule.ModuleId
        };

        return await _lessonService.CreateOneAsync(createLessonDto);
    }
    #endregion
}

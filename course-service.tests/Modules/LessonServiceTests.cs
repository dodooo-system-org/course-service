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

public class LessonServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ILessonService _lessonService;
    private readonly IModuleService _moduleService;
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private readonly ICourseCachingService _courseCachingService;

    public LessonServiceTests()
    {
        // Configure test logger to avoid logging issues
        LoggerHelper.SetLoggerFactory(new NoOpLoggerFactory());

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _categoryService = new CategoryService(_context);
        _courseCachingService = new CourseCachingService(new Mock<IDistributedCache>().Object, new Mock<ICacheManager>().Object);
        _courseService = new CourseService(_context, _categoryService, _courseCachingService);
        _moduleService = new ModuleService(_context, _courseService);
        _lessonService = new LessonService(_context, _moduleService);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    #region CreateOneAsync Tests
    [Fact]
    public async Task CreateOneAsync_CreateSuccessfully_ReturnsLessonDto()
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

        var createLessonDto = new CreateLessonDto
        {
            LessonName = "Test Lesson",
            LessonDescription = "Test lesson description for testing purposes",
            Order = 1,
            Duration = 30,
            ModuleId = createdModule.ModuleId
        };

        // Act
        var result = await _lessonService.CreateOneAsync(createLessonDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createLessonDto.LessonName, result.LessonName);
        Assert.Equal(createLessonDto.LessonDescription, result.LessonDescription);
        Assert.Equal(createLessonDto.Order, result.Order);
        Assert.Equal(createLessonDto.Duration, result.Duration);
        Assert.Equal(createLessonDto.ModuleId, result.ModuleId);
    }

    [Fact]
    public async Task CreateOneAsync_ModuleNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var createLessonDto = new CreateLessonDto
        {
            LessonName = "Test Lesson",
            LessonDescription = "Test lesson description for testing purposes",
            Order = 1,
            Duration = 30,
            ModuleId = Guid.NewGuid() // Non-existent module ID
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _lessonService.CreateOneAsync(createLessonDto));

        Assert.Contains("Module not found", exception.Message);
    }

    [Fact]
    public async Task CreateOneAsync_DuplicateOrder_ThrowsArgumentException()
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

        // Create first lesson with order 1
        var firstLessonDto = new CreateLessonDto
        {
            LessonName = "First Lesson",
            LessonDescription = "First lesson description for testing purposes",
            Order = 1,
            Duration = 30,
            ModuleId = createdModule.ModuleId
        };
        await _lessonService.CreateOneAsync(firstLessonDto);

        // Create second lesson with same order 1
        var duplicateOrderLessonDto = new CreateLessonDto
        {
            LessonName = "Second Lesson",
            LessonDescription = "Second lesson description for testing purposes",
            Order = 1, // Same order as first lesson
            Duration = 45,
            ModuleId = createdModule.ModuleId
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _lessonService.CreateOneAsync(duplicateOrderLessonDto));

        Assert.Equal("Order was existed", exception.Message);
    }
    #endregion

    #region GetAllAsync Tests
    [Fact]
    public async Task GetAllAsync_SuccessfullyReturnsLessonsForModule_ReturnsListOfLessons()
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

        // Create multiple lessons for the module
        var firstLessonDto = new CreateLessonDto
        {
            LessonName = "First Lesson",
            LessonDescription = "First lesson description for testing purposes",
            Order = 1,
            Duration = 30,
            ModuleId = createdModule.ModuleId
        };
        await _lessonService.CreateOneAsync(firstLessonDto);

        var secondLessonDto = new CreateLessonDto
        {
            LessonName = "Second Lesson",
            LessonDescription = "Second lesson description for testing purposes",
            Order = 2,
            Duration = 45,
            ModuleId = createdModule.ModuleId
        };
        await _lessonService.CreateOneAsync(secondLessonDto);

        // Act
        var result = await _lessonService.GetAllAsync(createdModule.ModuleId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, l => l.LessonName == firstLessonDto.LessonName);
        Assert.Contains(result, l => l.LessonName == secondLessonDto.LessonName);
        Assert.All(result, l => Assert.Equal(createdModule.ModuleId, l.ModuleId));
    }

    [Fact]
    public async Task GetAllAsync_ModuleNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentModuleId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _lessonService.GetAllAsync(nonExistentModuleId));

        Assert.Contains("Module not found", exception.Message);
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ReturnsEmptyList()
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

        // Act (no lessons created for this module)
        var result = await _lessonService.GetAllAsync(createdModule.ModuleId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    #endregion

    #region GetOneByIdAsync Tests
    [Fact]
    public async Task GetOneByIdAsync_SuccessfullyReturnsLesson_ReturnsLessonDto()
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

        var createLessonDto = new CreateLessonDto
        {
            LessonName = "Test Lesson",
            LessonDescription = "Test lesson description for testing purposes",
            Order = 1,
            Duration = 30,
            ModuleId = createdModule.ModuleId
        };
        var createdLesson = await _lessonService.CreateOneAsync(createLessonDto);

        // Act
        var result = await _lessonService.GetOneByIdAsync(createdLesson.LessonId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdLesson.LessonId, result.LessonId);
        Assert.Equal(createLessonDto.LessonName, result.LessonName);
        Assert.Equal(createLessonDto.LessonDescription, result.LessonDescription);
        Assert.Equal(createLessonDto.Order, result.Order);
        Assert.Equal(createLessonDto.Duration, result.Duration);
        Assert.Equal(createLessonDto.ModuleId, result.ModuleId);
    }

    [Fact]
    public async Task GetOneByIdAsync_LessonNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentLessonId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _lessonService.GetOneByIdAsync(nonExistentLessonId));

        Assert.Equal("Lesson not found", exception.Message);
    }

    [Fact]
    public async Task GetOneByIdAsync_EmptyId_ThrowsArgumentException()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _lessonService.GetOneByIdAsync(emptyId));

        Assert.Equal("Lesson ID cannot be empty", exception.Message);
    }
    #endregion

    #region UpdateOneAsync Tests
    [Fact]
    public async Task UpdateOneAsync_SuccessfulUpdate_ReturnsUpdatedLessonDto()
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

        var createLessonDto = new CreateLessonDto
        {
            LessonName = "Original Lesson",
            LessonDescription = "Original lesson description for testing purposes",
            Order = 1,
            Duration = 30,
            ModuleId = createdModule.ModuleId
        };
        var createdLesson = await _lessonService.CreateOneAsync(createLessonDto);

        var updateLessonDto = new UpdateLessonDto
        {
            LessonName = "Updated Lesson",
            LessonDescription = "Updated lesson description for testing purposes",
            Order = 2,
            Duration = 45,
            ModuleId = createdModule.ModuleId
        };

        // Act
        var result = await _lessonService.UpdateOneAsync(createdLesson.LessonId, updateLessonDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdLesson.LessonId, result.LessonId);
        Assert.Equal(updateLessonDto.LessonName, result.LessonName);
        Assert.Equal(updateLessonDto.LessonDescription, result.LessonDescription);
        Assert.Equal(updateLessonDto.Order, result.Order);
        Assert.Equal(updateLessonDto.Duration, result.Duration);
    }

    [Fact]
    public async Task UpdateOneAsync_LessonNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentLessonId = Guid.NewGuid();
        var updateLessonDto = new UpdateLessonDto
        {
            LessonName = "Updated Lesson",
            LessonDescription = "Updated lesson description for testing purposes",
            Order = 1,
            Duration = 30,
            ModuleId = Guid.NewGuid()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _lessonService.UpdateOneAsync(nonExistentLessonId, updateLessonDto));

        Assert.Contains("Lesson not found", exception.Message);
    }

    [Fact]
    public async Task UpdateOneAsync_DuplicateOrder_ThrowsArgumentException()
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

        // Create first lesson with order 1
        var firstLessonDto = new CreateLessonDto
        {
            LessonName = "First Lesson",
            LessonDescription = "First lesson description for testing purposes",
            Order = 1,
            Duration = 30,
            ModuleId = createdModule.ModuleId
        };
        var firstLesson = await _lessonService.CreateOneAsync(firstLessonDto);

        // Create second lesson with order 2
        var secondLessonDto = new CreateLessonDto
        {
            LessonName = "Second Lesson",
            LessonDescription = "Second lesson description for testing purposes",
            Order = 2,
            Duration = 45,
            ModuleId = createdModule.ModuleId
        };
        var secondLesson = await _lessonService.CreateOneAsync(secondLessonDto);

        // Try to update second lesson to have same order as first lesson
        var updateLessonDto = new UpdateLessonDto
        {
            LessonName = "Updated Second Lesson",
            LessonDescription = "Updated second lesson description",
            Order = 1, // Same order as first lesson
            Duration = 50,
            ModuleId = createdModule.ModuleId
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _lessonService.UpdateOneAsync(secondLesson.LessonId, updateLessonDto));

        Assert.Contains("Order was existed", exception.Message);
    }

    [Fact]
    public async Task UpdateOneAsync_EmptyLessonId_ThrowsArgumentException()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var updateLessonDto = new UpdateLessonDto
        {
            LessonName = "Updated Lesson",
            LessonDescription = "Updated lesson description for testing purposes",
            Order = 1,
            Duration = 30,
            ModuleId = Guid.NewGuid()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _lessonService.UpdateOneAsync(emptyId, updateLessonDto));

        Assert.Contains("Lesson ID cannot be empty", exception.Message);
    }
    #endregion
}

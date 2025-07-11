using System;
using course_service.Data;
using course_service.Data.Entities;
using course_service.Modules.Category.DTOs;
using course_service.Modules.Category.Interfaces;
using course_service.Modules.Category.Services;
using course_service.Modules.Course.DTOs;
using course_service.Modules.Course.Interfaces;
using course_service.Modules.Course.Services;
using Microsoft.EntityFrameworkCore;

namespace course_service.tests;

public class CourseServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    public CourseServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _categoryService = new CategoryService(_context);
        _courseService = new CourseService(_context, _categoryService);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region CreateCourseAsync
    [Fact]
    public async Task CreateCourseAsync_CreatedCourse_ReturnsCourse()
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

        // Act
        var createdCourse = await _courseService.CreateCourseAsync(createCourseDto);

        // Assert
        Assert.NotNull(createdCourse);
        Assert.Equal(createCourseDto.CourseName, createdCourse.CourseName);
        Assert.Equal(createCourseDto.CourseDescription, createdCourse.CourseDescription);
        Assert.Equal(createCourseDto.CourseImageUrl, createdCourse.CourseImageUrl);
        Assert.Equal(createCourseDto.CourseLevel, createdCourse.Level);
        Assert.Equal(createdCategory.CategoryId, createdCourse.CategoryId);
        Assert.NotNull(createdCourse.Category);
    }

    [Fact]
    public async Task CreateCourseAsync_CategoryNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var createCourseDto = new CreateCourseDto
        {
            CourseName = "Test Course",
            CourseDescription = "Test Description",
            CourseImageUrl = "http://example.com/course.jpg",
            CourseLevel = CourseLevel.Beginner,
            CategoryId = Guid.NewGuid() // Non-existent category ID
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _courseService.CreateCourseAsync(createCourseDto));
    }
    #endregion

    #region GetAllCoursesAsync
    [Fact]
    public async Task GetAllCoursesAsync_ReturnsAllCourses()
    {
        // Arrange
        var categoryEntity = new CategoryEntity
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        _context.Categories.Add(categoryEntity);

        var courseEntity1 = new CourseEntity
        {
            CourseName = "Test Course 1",
            CourseDescription = "Test Description 1",
            CourseImageUrl = "http://example.com/course1.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = categoryEntity.CategoryId,
        };
        var courseEntity2 = new CourseEntity
        {
            CourseName = "Test Course 2",
            CourseDescription = "Test Description 2",
            CourseImageUrl = "http://example.com/course2.jpg",
            Level = CourseLevel.Intermediate,
            CategoryId = categoryEntity.CategoryId,
        };

        _context.Courses.AddRange(courseEntity1, courseEntity2);
        await _context.SaveChangesAsync();

        // Act
        var courses = await _courseService.GetAllCoursesAsync();

        // Assert
        Assert.NotNull(courses);
        Assert.Equal(2, courses.Count);
        Assert.Contains(courses, c => c.CourseName == "Test Course 1" && c.Category.CategoryId == categoryEntity.CategoryId);
        Assert.Contains(courses, c => c.CourseName == "Test Course 2" && c.Category.CategoryId == categoryEntity.CategoryId);
    }

    [Fact]
    public async Task GetAllCoursesAsync_NoCourses_ReturnsEmptyList()
    {
        // Arrange
        // Ensure the database of courses is empty
        // Act
        var courses = await _courseService.GetAllCoursesAsync();

        // Assert
        Assert.NotNull(courses);
        Assert.Empty(courses);
    }
    #endregion

    #region GetCourseByIdAsync
    [Fact]
    public async Task GetCourseByIdAsync_ValidId_ReturnsCourse()
    {
        // Arrange
        var categoryEntity = new CategoryEntity
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        _context.Categories.Add(categoryEntity);

        var courseEntity1 = new CourseEntity
        {
            CourseName = "Test Course 1",
            CourseDescription = "Test Description 1",
            CourseImageUrl = "http://example.com/course1.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = categoryEntity.CategoryId,
        };
        var courseEntity2 = new CourseEntity
        {
            CourseName = "Test Course 2",
            CourseDescription = "Test Description 2",
            CourseImageUrl = "http://example.com/course2.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = categoryEntity.CategoryId,
        };
        _context.Courses.AddRange(courseEntity1, courseEntity2);
        await _context.SaveChangesAsync();

        // Act
        var course = await _courseService.GetCourseByIdAsync(courseEntity1.CourseId);

        // Assert
        Assert.NotNull(course);
        Assert.Equal(courseEntity1.CourseName, course.CourseName);
        Assert.Equal(courseEntity1.CourseDescription, course.CourseDescription);
        Assert.Equal(courseEntity1.CourseImageUrl, course.CourseImageUrl);
        Assert.Equal(courseEntity1.Level, course.Level);
        Assert.Equal(categoryEntity.CategoryId, course.Category.CategoryId);
    }

    [Fact]
    public async Task GetCourseByIdAsync_InvalidId_ThrowsArgumentException()
    {
        // Arrange
        var invalidCourseId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _courseService.GetCourseByIdAsync(invalidCourseId));
        Assert.Equal("Course ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task GetCourseByIdAsync_NotExistedCourse_ThrowsKeyNotFound()
    {
        // Arrange
        var nonExistentCourseId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _courseService.GetCourseByIdAsync(nonExistentCourseId));
        Assert.Equal("Course not found", exception.Message);
    }
    #endregion

    #region UpdateCourseAsync
    [Fact]
    public async Task UpdateCourseAsync_ValidData_UpdatesCourseSuccessfully()
    {
        // Arrange
        var categoryEntity1 = new CategoryEntity
        {
            CategoryName = "Test Category 1",
            CategoryDescription = "Test Description 1",
            CategoryImageUrl = "http://example.com/image1.jpg"
        };
        var categoryEntity2 = new CategoryEntity
        {
            CategoryName = "Test Category 2",
            CategoryDescription = "Test Description 2",
            CategoryImageUrl = "http://example.com/image2.jpg"
        };
        _context.Categories.AddRange(categoryEntity1, categoryEntity2);

        var courseEntity = new CourseEntity
        {
            CourseName = "Original Course",
            CourseDescription = "Original Description",
            CourseImageUrl = "http://example.com/original.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = categoryEntity1.CategoryId,
        };
        _context.Courses.Add(courseEntity);
        await _context.SaveChangesAsync();

        var updateCourseDto = new UpdateCourseDto
        {
            CourseName = "Updated Course Name",
            CourseDescription = "Updated Course Description",
            CourseImageUrl = "http://example.com/updated.jpg",
            CourseLevel = CourseLevel.Advanced,
            CategoryId = categoryEntity2.CategoryId
        };

        // Act
        var updatedCourse = await _courseService.UpdateCourseAsync(courseEntity.CourseId, updateCourseDto);

        // Assert
        Assert.NotNull(updatedCourse);
        Assert.Equal(updateCourseDto.CourseName, updatedCourse.CourseName);
        Assert.Equal(updateCourseDto.CourseDescription, updatedCourse.CourseDescription);
        Assert.Equal(updateCourseDto.CourseImageUrl, updatedCourse.CourseImageUrl);
        Assert.Equal(updateCourseDto.CourseLevel, updatedCourse.Level);
        Assert.Equal(updateCourseDto.CategoryId, updatedCourse.Category.CategoryId);

        // Verify the course was actually updated in the database
        var courseInDb = await _context.Courses.FindAsync(courseEntity.CourseId);
        Assert.NotNull(courseInDb);
        Assert.Equal(updateCourseDto.CourseName, courseInDb.CourseName);
        Assert.Equal(updateCourseDto.CourseDescription, courseInDb.CourseDescription);
        Assert.Equal(updateCourseDto.CourseImageUrl, courseInDb.CourseImageUrl);
        Assert.Equal(updateCourseDto.CourseLevel, courseInDb.Level);
        Assert.Equal(updateCourseDto.CategoryId, courseInDb.CategoryId);
        Assert.True(courseInDb.UpdatedAt > courseEntity.CreatedAt);
    }

    [Fact]
    public async Task UpdateCourseAsync_InvalidCourseId_ThrowsArgumentException()
    {
        // Arrange
        var invalidCourseId = Guid.Empty;
        var updateCourseDto = new UpdateCourseDto
        {
            CourseName = "Updated Course Name",
            CourseDescription = "Updated Course Description",
            CourseImageUrl = "http://example.com/updated.jpg",
            CourseLevel = CourseLevel.Advanced,
            CategoryId = Guid.NewGuid()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _courseService.UpdateCourseAsync(invalidCourseId, updateCourseDto));
        Assert.Contains("Course ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task UpdateCourseAsync_NonExistentCourseId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var categoryEntity = new CategoryEntity
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        _context.Categories.Add(categoryEntity);
        await _context.SaveChangesAsync();

        var nonExistentCourseId = Guid.NewGuid();
        var updateCourseDto = new UpdateCourseDto
        {
            CourseName = "Updated Course Name",
            CourseDescription = "Updated Course Description",
            CourseImageUrl = "http://example.com/updated.jpg",
            CourseLevel = CourseLevel.Advanced,
            CategoryId = categoryEntity.CategoryId
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _courseService.UpdateCourseAsync(nonExistentCourseId, updateCourseDto));
        Assert.Equal($"Course not found", exception.Message);
    }

    [Fact]
    public async Task UpdateCourseAsync_NonExistentCategoryId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var categoryEntity = new CategoryEntity
        {
            CategoryName = "Test Category",
            CategoryDescription = "Test Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        _context.Categories.Add(categoryEntity);

        var courseEntity = new CourseEntity
        {
            CourseName = "Original Course",
            CourseDescription = "Original Description",
            CourseImageUrl = "http://example.com/original.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = categoryEntity.CategoryId,
        };
        _context.Courses.Add(courseEntity);
        await _context.SaveChangesAsync();

        var nonExistentCategoryId = Guid.NewGuid();
        var updateCourseDto = new UpdateCourseDto
        {
            CourseName = "Updated Course Name",
            CourseDescription = "Updated Course Description",
            CourseImageUrl = "http://example.com/updated.jpg",
            CourseLevel = CourseLevel.Advanced,
            CategoryId = nonExistentCategoryId
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _courseService.UpdateCourseAsync(courseEntity.CourseId, updateCourseDto));
        Assert.Equal("Category not found", exception.Message);
    }
    #endregion
}
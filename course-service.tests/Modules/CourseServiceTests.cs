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
using course_service.Shared.DTOs;
using course_service.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace course_service.tests;

public class CourseServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private readonly ICourseCachingService _courseCachingService;
    public CourseServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        var mockCategoryCaching = new Mock<ICategoryCachingService>();
        _categoryService = new CategoryService(_context, mockCategoryCaching.Object);

        _courseCachingService = new CourseCachingService(new Mock<IDistributedCache>().Object, new Mock<ICacheManager>().Object);
        _courseService = new CourseService(_context, _categoryService, _courseCachingService);
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
        var result = await _courseService.GetAllCoursesAsync(new AllCourseQueryDto());

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Contains(result.Data, c => c.CourseName == "Test Course 1" && c.Category.CategoryId == categoryEntity.CategoryId);
        Assert.Contains(result.Data, c => c.CourseName == "Test Course 2" && c.Category.CategoryId == categoryEntity.CategoryId);

        // Assert metadata
        Assert.NotNull(result.Meta);
        Assert.Equal(1, result.Meta.Page);
        Assert.Equal(10, result.Meta.Size);
        Assert.Equal(2, result.Meta.TotalCount);
    }

    [Fact]
    public async Task GetAllCoursesAsync_NoCourses_ReturnsEmptyList()
    {
        // Arrange
        // Ensure the database of courses is empty
        // Act
        var result = await _courseService.GetAllCoursesAsync(new AllCourseQueryDto());

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);

        // Assert metadata
        Assert.NotNull(result.Meta);
        Assert.Equal(1, result.Meta.Page);
        Assert.Equal(10, result.Meta.Size);
        Assert.Equal(0, result.Meta.TotalCount);
    }

    [Fact]
    public async Task GetAllCoursesAsync_FilterByCategoryId_ReturnsMatchingCourses()
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

        var course1 = new CourseEntity
        {
            CourseName = "Course in Category 1",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course1.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = category1.CategoryId
        };
        var course2 = new CourseEntity
        {
            CourseName = "Course in Category 2",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course2.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = category2.CategoryId
        };
        _context.Courses.AddRange(course1, course2);
        await _context.SaveChangesAsync();

        var queryDto = new AllCourseQueryDto { CategoryId = category1.CategoryId };

        // Act
        var result = await _courseService.GetAllCoursesAsync(queryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal("Course in Category 1", result.Data[0].CourseName);
        Assert.Equal(category1.CategoryId, result.Data[0].Category.CategoryId);
        Assert.Equal(1, result.Meta.TotalCount);
    }

    [Fact]
    public async Task GetAllCoursesAsync_FilterByCourseLevel_ReturnsMatchingCourses()
    {
        // Arrange
        var category = new CategoryEntity
        {
            CategoryName = "Test Category",
            CategoryDescription = "Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        _context.Categories.Add(category);

        var beginnerCourse = new CourseEntity
        {
            CourseName = "Beginner Course",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course1.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = category.CategoryId
        };
        var advancedCourse = new CourseEntity
        {
            CourseName = "Advanced Course",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course2.jpg",
            Level = CourseLevel.Advanced,
            CategoryId = category.CategoryId
        };
        _context.Courses.AddRange(beginnerCourse, advancedCourse);
        await _context.SaveChangesAsync();

        var queryDto = new AllCourseQueryDto { CourseLevel = CourseLevel.Advanced };

        // Act
        var result = await _courseService.GetAllCoursesAsync(queryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal("Advanced Course", result.Data[0].CourseName);
        Assert.Equal(CourseLevel.Advanced, result.Data[0].CourseLevel);
        Assert.Equal(1, result.Meta.TotalCount);
    }

    [Fact]
    public async Task GetAllCoursesAsync_FilterByIsActive_ReturnsMatchingCourses()
    {
        // Arrange
        var category = new CategoryEntity
        {
            CategoryName = "Test Category",
            CategoryDescription = "Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        _context.Categories.Add(category);

        var activeCourse = new CourseEntity
        {
            CourseName = "Active Course",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course1.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = category.CategoryId,
            IsActive = true
        };
        var inactiveCourse = new CourseEntity
        {
            CourseName = "Inactive Course",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course2.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = category.CategoryId,
            IsActive = false
        };
        _context.Courses.AddRange(activeCourse, inactiveCourse);
        await _context.SaveChangesAsync();

        var queryDto = new AllCourseQueryDto { IsActive = true };

        // Act
        var result = await _courseService.GetAllCoursesAsync(queryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal("Active Course", result.Data[0].CourseName);
        Assert.Equal(1, result.Meta.TotalCount);
    }

    [Fact]
    public async Task GetAllCoursesAsync_FilterByIsDeleted_ReturnsMatchingCourses()
    {
        // Arrange
        var category = new CategoryEntity
        {
            CategoryName = "Test Category",
            CategoryDescription = "Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        _context.Categories.Add(category);

        var normalCourse = new CourseEntity
        {
            CourseName = "Normal Course",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course1.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = category.CategoryId,
            IsDeleted = false
        };
        var deletedCourse = new CourseEntity
        {
            CourseName = "Deleted Course",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course2.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = category.CategoryId,
            IsDeleted = true
        };
        _context.Courses.AddRange(normalCourse, deletedCourse);
        await _context.SaveChangesAsync();

        var queryDto = new AllCourseQueryDto { IsDeleted = true };

        // Act
        var result = await _courseService.GetAllCoursesAsync(queryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal("Deleted Course", result.Data[0].CourseName);
        Assert.Equal(1, result.Meta.TotalCount);
    }

    [Fact]
    public async Task GetAllCoursesAsync_DefaultIsDeletedFilter_ExcludesDeletedCourses()
    {
        // Arrange
        var category = new CategoryEntity
        {
            CategoryName = "Test Category",
            CategoryDescription = "Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        _context.Categories.Add(category);

        var normalCourse = new CourseEntity
        {
            CourseName = "Normal Course",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course1.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = category.CategoryId,
            IsDeleted = false
        };
        var deletedCourse = new CourseEntity
        {
            CourseName = "Deleted Course",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course2.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = category.CategoryId,
            IsDeleted = true
        };
        _context.Courses.AddRange(normalCourse, deletedCourse);
        await _context.SaveChangesAsync();

        var queryDto = new AllCourseQueryDto(); // No IsDeleted filter specified

        // Act
        var result = await _courseService.GetAllCoursesAsync(queryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal("Normal Course", result.Data[0].CourseName);
        Assert.Equal(1, result.Meta.TotalCount);
    }

    [Fact]
    public async Task GetAllCoursesAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var category = new CategoryEntity
        {
            CategoryName = "Test Category",
            CategoryDescription = "Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        _context.Categories.Add(category);

        // Create 5 courses
        var courses = new List<CourseEntity>();
        for (int i = 1; i <= 5; i++)
        {
            courses.Add(new CourseEntity
            {
                CourseName = $"Course {i}",
                CourseDescription = "Description",
                CourseImageUrl = $"http://example.com/course{i}.jpg",
                Level = CourseLevel.Beginner,
                CategoryId = category.CategoryId
            });
        }
        _context.Courses.AddRange(courses);
        await _context.SaveChangesAsync();

        var queryDto = new AllCourseQueryDto { Page = 2, Size = 2 };

        // Act
        var result = await _courseService.GetAllCoursesAsync(queryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Meta.Page);
        Assert.Equal(2, result.Meta.Size);
        Assert.Equal(5, result.Meta.TotalCount);
    }

    [Fact]
    public async Task GetAllCoursesAsync_WithModulesAndLessons_ReturnsCorrectCounts()
    {
        // Arrange
        var category = new CategoryEntity
        {
            CategoryName = "Test Category",
            CategoryDescription = "Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        _context.Categories.Add(category);

        var course = new CourseEntity
        {
            CourseName = "Test Course",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = category.CategoryId
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        // Create 2 modules for the course
        var module1 = new ModuleEntity
        {
            ModuleName = "Module 1",
            ModuleDescription = "Description",
            CourseId = course.CourseId,
            Order = 1
        };
        var module2 = new ModuleEntity
        {
            ModuleName = "Module 2",
            ModuleDescription = "Description",
            CourseId = course.CourseId,
            Order = 2
        };
        _context.Modules.AddRange(module1, module2);
        await _context.SaveChangesAsync();

        // Create lessons for modules
        var lesson1 = new LessonEntity
        {
            LessonName = "Lesson 1",
            LessonDescription = "Description",
            ModuleId = module1.ModuleId,
            Order = 1,
            Duration = 30
        };
        var lesson2 = new LessonEntity
        {
            LessonName = "Lesson 2",
            LessonDescription = "Description",
            ModuleId = module1.ModuleId,
            Order = 2,
            Duration = 45
        };
        var lesson3 = new LessonEntity
        {
            LessonName = "Lesson 3",
            LessonDescription = "Description",
            ModuleId = module2.ModuleId,
            Order = 1,
            Duration = 60
        };
        _context.Lessons.AddRange(lesson1, lesson2, lesson3);
        await _context.SaveChangesAsync();

        var queryDto = new AllCourseQueryDto();

        // Act
        var result = await _courseService.GetAllCoursesAsync(queryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        var courseDto = result.Data[0];
        Assert.Equal("Test Course", courseDto.CourseName);
        Assert.Equal(2, courseDto.ModuleCount);
        Assert.Equal(3, courseDto.LessonCount);
    }

    [Fact]
    public async Task GetAllCoursesAsync_WithDeletedModulesAndLessons_ExcludesDeletedFromCounts()
    {
        // Arrange
        var category = new CategoryEntity
        {
            CategoryName = "Test Category",
            CategoryDescription = "Description",
            CategoryImageUrl = "http://example.com/image.jpg"
        };
        _context.Categories.Add(category);

        var course = new CourseEntity
        {
            CourseName = "Test Course",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course.jpg",
            Level = CourseLevel.Beginner,
            CategoryId = category.CategoryId
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        // Create modules (1 active, 1 deleted)
        var activeModule = new ModuleEntity
        {
            ModuleName = "Active Module",
            ModuleDescription = "Description",
            CourseId = course.CourseId,
            Order = 1,
            IsDeleted = false
        };
        var deletedModule = new ModuleEntity
        {
            ModuleName = "Deleted Module",
            ModuleDescription = "Description",
            CourseId = course.CourseId,
            Order = 2,
            IsDeleted = true
        };
        _context.Modules.AddRange(activeModule, deletedModule);
        await _context.SaveChangesAsync();

        // Create lessons (2 active in active module, 1 deleted in active module, 1 active in deleted module)
        var activeLesson1 = new LessonEntity
        {
            LessonName = "Active Lesson 1",
            LessonDescription = "Description",
            ModuleId = activeModule.ModuleId,
            Order = 1,
            Duration = 30,
            IsDeleted = false
        };
        var activeLesson2 = new LessonEntity
        {
            LessonName = "Active Lesson 2",
            LessonDescription = "Description",
            ModuleId = activeModule.ModuleId,
            Order = 2,
            Duration = 45,
            IsDeleted = false
        };
        var deletedLesson = new LessonEntity
        {
            LessonName = "Deleted Lesson",
            LessonDescription = "Description",
            ModuleId = activeModule.ModuleId,
            Order = 3,
            Duration = 60,
            IsDeleted = true
        };
        var lessonInDeletedModule = new LessonEntity
        {
            LessonName = "Lesson in Deleted Module",
            LessonDescription = "Description",
            ModuleId = deletedModule.ModuleId,
            Order = 1,
            Duration = 30,
            IsDeleted = false
        };
        _context.Lessons.AddRange(activeLesson1, activeLesson2, deletedLesson, lessonInDeletedModule);
        await _context.SaveChangesAsync();

        var queryDto = new AllCourseQueryDto();

        // Act
        var result = await _courseService.GetAllCoursesAsync(queryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        var courseDto = result.Data[0];
        Assert.Equal("Test Course", courseDto.CourseName);
        Assert.Equal(1, courseDto.ModuleCount); // Only active module
        Assert.Equal(2, courseDto.LessonCount); // Only active lessons in active modules
    }

    [Fact]
    public async Task GetAllCoursesAsync_CombinedFilters_ReturnsMatchingCourses()
    {
        // Arrange
        var category1 = new CategoryEntity
        {
            CategoryName = "Category 1",
            CategoryDescription = "Description",
            CategoryImageUrl = "http://example.com/image1.jpg"
        };
        var category2 = new CategoryEntity
        {
            CategoryName = "Category 2",
            CategoryDescription = "Description",
            CategoryImageUrl = "http://example.com/image2.jpg"
        };
        _context.Categories.AddRange(category1, category2);

        var matchingCourse = new CourseEntity
        {
            CourseName = "Matching Course",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course1.jpg",
            Level = CourseLevel.Advanced,
            CategoryId = category1.CategoryId,
            IsActive = true,
            IsDeleted = false
        };
        var nonMatchingCourse1 = new CourseEntity
        {
            CourseName = "Non-matching Course 1",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course2.jpg",
            Level = CourseLevel.Beginner, // Different level
            CategoryId = category1.CategoryId,
            IsActive = true,
            IsDeleted = false
        };
        var nonMatchingCourse2 = new CourseEntity
        {
            CourseName = "Non-matching Course 2",
            CourseDescription = "Description",
            CourseImageUrl = "http://example.com/course3.jpg",
            Level = CourseLevel.Advanced,
            CategoryId = category2.CategoryId, // Different category
            IsActive = true,
            IsDeleted = false
        };
        _context.Courses.AddRange(matchingCourse, nonMatchingCourse1, nonMatchingCourse2);
        await _context.SaveChangesAsync();

        var queryDto = new AllCourseQueryDto
        {
            CategoryId = category1.CategoryId,
            CourseLevel = CourseLevel.Advanced,
            IsActive = true
        };

        // Act
        var result = await _courseService.GetAllCoursesAsync(queryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal("Matching Course", result.Data[0].CourseName);
        Assert.Equal(1, result.Meta.TotalCount);
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
        Assert.Equal(courseEntity1.Level, course.CourseLevel);
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
        Assert.Equal(updateCourseDto.CourseLevel, updatedCourse.CourseLevel);
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
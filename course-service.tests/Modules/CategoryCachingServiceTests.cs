using System;
using System.Text;
using course_service.Data.Entities;
using course_service.Modules.Caching.Services;
using course_service.Shared.DTOs;
using course_service.Shared.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace course_service.tests.Modules;

public class CategoryCachingServiceTests : IDisposable
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ICacheManager> _mockCacheManager;
    private readonly CategoryCachingService _service;
    private readonly string _uniqueKey = "test-unique-key";

    public CategoryCachingServiceTests()
    {
        // Disable logging for tests
        Shared.Helpers.LoggerHelper.SetLoggerFactory(new NoOpLoggerFactory());

        _mockCache = new Mock<IDistributedCache>();
        _mockCacheManager = new Mock<ICacheManager>();
        _service = new CategoryCachingService(_mockCache.Object, _mockCacheManager.Object);
    }

    public void Dispose()
    {
        // Clean up if needed
    }

    #region CacheListAllCategoriesAsync Tests

    [Fact]
    public async Task CacheListAllCategoriesAsync_ValidData_ReturnsTrue()
    {
        // Arrange
        var categories = CreateSampleCategories();
        var metaPagination = new MetaPaginationDto<List<CategoryEntity>>
        {
            Data = categories,
            Meta = new MetaDto { Page = 1, Size = 10, TotalCount = 2 }
        };

        _mockCache.Setup(x => x.SetAsync(
            It.Is<string>(key => key == $"all_categories:{_uniqueKey}"),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CacheListAllCategoriesAsync(metaPagination, _uniqueKey);

        // Assert
        Assert.True(result);
        _mockCache.Verify(x => x.SetAsync(
            It.Is<string>(key => key == $"all_categories:{_uniqueKey}"),
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(opts => opts.AbsoluteExpirationRelativeToNow == TimeSpan.FromHours(1)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CacheListAllCategoriesAsync_CacheThrowsException_ReturnsNull()
    {
        // Arrange
        var categories = CreateSampleCategories();
        var metaPagination = new MetaPaginationDto<List<CategoryEntity>>
        {
            Data = categories,
            Meta = new MetaDto { Page = 1, Size = 10, TotalCount = 2 }
        };

        _mockCache.Setup(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache error"));

        // Act
        var result = await _service.CacheListAllCategoriesAsync(metaPagination, _uniqueKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CacheListAllCategoriesAsync_NullData_HandlesGracefully()
    {
        // Act
        var result = await _service.CacheListAllCategoriesAsync(null!, _uniqueKey);

        // Assert
        // The service serializes null as "null" string and caches it successfully
        Assert.True(result);
    }

    #endregion

    #region CacheListAvailableCategoriesAsync Tests

    [Fact]
    public async Task CacheListAvailableCategoriesAsync_ValidData_ReturnsTrue()
    {
        // Arrange
        var categories = CreateSampleCategories();

        _mockCache.Setup(x => x.SetAsync(
            It.Is<string>(key => key == "all_categories:available"),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CacheListAvailableCategoriesAsync(categories);

        // Assert
        Assert.True(result);
        _mockCache.Verify(x => x.SetAsync(
            It.Is<string>(key => key == "all_categories:available"),
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(opts => opts.AbsoluteExpirationRelativeToNow == TimeSpan.FromHours(1)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CacheListAvailableCategoriesAsync_CacheThrowsException_ReturnsNull()
    {
        // Arrange
        var categories = CreateSampleCategories();

        _mockCache.Setup(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache error"));

        // Act
        var result = await _service.CacheListAvailableCategoriesAsync(categories);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CacheListAvailableCategoriesAsync_NullData_HandlesGracefully()
    {
        // Act
        var result = await _service.CacheListAvailableCategoriesAsync(null!);

        // Assert
        // The service serializes null as "null" string and caches it successfully
        Assert.True(result);
    }

    #endregion

    #region GetListAllCategoriesAsync Tests

    [Fact]
    public async Task GetListAllCategoriesAsync_ValidCachedData_ReturnsDeserializedData()
    {
        // Arrange
        var categories = CreateSampleCategories();
        var metaPagination = new MetaPaginationDto<List<CategoryEntity>>
        {
            Data = categories,
            Meta = new MetaDto { Page = 1, Size = 10, TotalCount = 2 }
        };
        var serializedData = JsonConvert.SerializeObject(metaPagination);
        var cachedBytes = Encoding.UTF8.GetBytes(serializedData);

        _mockCache.Setup(x => x.GetAsync(
            It.Is<string>(key => key == $"all_categories:{_uniqueKey}"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        // Act
        var result = await _service.GetListAllCategoriesAsync(_uniqueKey);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal("Technology", result.Data[0].CategoryName);
        Assert.Equal("Science", result.Data[1].CategoryName);
        Assert.Equal(1, result.Meta.Page);
        Assert.Equal(10, result.Meta.Size);
        Assert.Equal(2, result.Meta.TotalCount);
    }

    [Fact]
    public async Task GetListAllCategoriesAsync_NoCachedData_ReturnsNull()
    {
        // Arrange
        _mockCache.Setup(x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _service.GetListAllCategoriesAsync(_uniqueKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetListAllCategoriesAsync_EmptyCachedData_ReturnsNull()
    {
        // Arrange
        _mockCache.Setup(x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[0]);

        // Act
        var result = await _service.GetListAllCategoriesAsync(_uniqueKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetListAllCategoriesAsync_CacheThrowsException_ReturnsNull()
    {
        // Arrange
        _mockCache.Setup(x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache error"));

        // Act
        var result = await _service.GetListAllCategoriesAsync(_uniqueKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetListAllCategoriesAsync_InvalidJson_ReturnsNull()
    {
        // Arrange
        var invalidJsonBytes = Encoding.UTF8.GetBytes("invalid json data");

        _mockCache.Setup(x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidJsonBytes);

        // Act
        var result = await _service.GetListAllCategoriesAsync(_uniqueKey);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetListAvailableCategoriesAsync Tests

    [Fact]
    public async Task GetListAvailableCategoriesAsync_ValidCachedData_ReturnsDeserializedData()
    {
        // Arrange
        var categories = CreateSampleCategories();
        var serializedData = JsonConvert.SerializeObject(categories);
        var cachedBytes = Encoding.UTF8.GetBytes(serializedData);

        _mockCache.Setup(x => x.GetAsync(
            It.Is<string>(key => key == "all_categories:available"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        // Act
        var result = await _service.GetListAvailableCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Technology", result[0].CategoryName);
        Assert.Equal("Science", result[1].CategoryName);
    }

    [Fact]
    public async Task GetListAvailableCategoriesAsync_NoCachedData_ReturnsNull()
    {
        // Arrange
        _mockCache.Setup(x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _service.GetListAvailableCategoriesAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetListAvailableCategoriesAsync_EmptyCachedData_ReturnsNull()
    {
        // Arrange
        _mockCache.Setup(x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[0]);

        // Act
        var result = await _service.GetListAvailableCategoriesAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetListAvailableCategoriesAsync_CacheThrowsException_ReturnsNull()
    {
        // Arrange
        _mockCache.Setup(x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache error"));

        // Act
        var result = await _service.GetListAvailableCategoriesAsync();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region RemoveListAllCategoriesAsync Tests

    [Fact]
    public async Task RemoveListAllCategoriesAsync_Success_ReturnsTrue()
    {
        // Arrange
        _mockCacheManager.Setup(x => x.RemoveByPattern("all_categories:*"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RemoveListAllCategoriesAsync();

        // Assert
        Assert.True(result);
        _mockCacheManager.Verify(x => x.RemoveByPattern("all_categories:*"), Times.Once);
    }

    [Fact]
    public async Task RemoveListAllCategoriesAsync_CacheManagerThrowsException_ReturnsNull()
    {
        // Arrange
        _mockCacheManager.Setup(x => x.RemoveByPattern(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Cache manager error"));

        // Act
        var result = await _service.RemoveListAllCategoriesAsync();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CacheAndRetrieve_AllCategories_RoundTripSuccessful()
    {
        // Arrange
        var categories = CreateSampleCategories();
        var metaPagination = new MetaPaginationDto<List<CategoryEntity>>
        {
            Data = categories,
            Meta = new MetaDto { Page = 1, Size = 10, TotalCount = 2 }
        };

        var serializedData = JsonConvert.SerializeObject(metaPagination);
        var cachedBytes = Encoding.UTF8.GetBytes(serializedData);

        _mockCache.Setup(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockCache.Setup(x => x.GetAsync(
            It.Is<string>(key => key == $"all_categories:{_uniqueKey}"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        // Act
        var cacheResult = await _service.CacheListAllCategoriesAsync(metaPagination, _uniqueKey);
        var retrieveResult = await _service.GetListAllCategoriesAsync(_uniqueKey);

        // Assert
        Assert.True(cacheResult);
        Assert.NotNull(retrieveResult);
        Assert.NotNull(retrieveResult.Data);
        Assert.Equal(2, retrieveResult.Data.Count);
        // Note: This test verifies the round-trip functionality works correctly
    }

    [Fact]
    public async Task CacheAndRetrieve_AvailableCategories_RoundTripSuccessful()
    {
        // Arrange
        var categories = CreateSampleCategories();
        var serializedData = JsonConvert.SerializeObject(categories);
        var cachedBytes = Encoding.UTF8.GetBytes(serializedData);

        _mockCache.Setup(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockCache.Setup(x => x.GetAsync(
            It.Is<string>(key => key == "all_categories:available"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        // Act
        var cacheResult = await _service.CacheListAvailableCategoriesAsync(categories);
        var retrieveResult = await _service.GetListAvailableCategoriesAsync();

        // Assert
        Assert.True(cacheResult);
        Assert.NotNull(retrieveResult);
        Assert.Equal(2, retrieveResult.Count);
        Assert.Equal("Technology", retrieveResult[0].CategoryName);
    }

    #endregion

    #region Helper Methods

    private List<CategoryEntity> CreateSampleCategories()
    {
        return new List<CategoryEntity>
        {
            new CategoryEntity
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = "Technology",
                CategoryDescription = "Technology related courses",
                CategoryImageUrl = "https://example.com/tech.jpg",
                Status = CategoryStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new CategoryEntity
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = "Science",
                CategoryDescription = "Science related courses",
                CategoryImageUrl = "https://example.com/science.jpg",
                Status = CategoryStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
    }

    #endregion
}

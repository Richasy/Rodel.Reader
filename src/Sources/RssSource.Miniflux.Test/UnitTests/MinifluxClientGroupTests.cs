// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Miniflux.Test.UnitTests;

/// <summary>
/// MinifluxClient 分组操作单元测试.
/// </summary>
[TestClass]
public sealed class MinifluxClientGroupTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private MinifluxClientOptions _options = null!;
    private MinifluxClient _client = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new MinifluxClient(_options, _httpClient);

        // 模拟登录
        _mockHandler.SetupResponse("/v1/me", TestDataFactory.CreateUserResponse());
        await _client.SignInAsync();
        _mockHandler.Clear();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public async Task AddGroupAsync_ShouldReturnNewGroup()
    {
        // Arrange
        var group = new RssFeedGroup
        {
            Name = "New Category",
        };
        _mockHandler.SetupResponse("/v1/categories", TestDataFactory.CreateCategoryResponse(10, "New Category"));

        // Act
        var result = await _client.AddGroupAsync(group);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("10", result.Id);
        Assert.AreEqual("New Category", result.Name);
    }

    [TestMethod]
    public async Task AddGroupAsync_WithNullGroup_ShouldThrow()
    {
        // Act & Assert
        _ = await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => _client.AddGroupAsync(null!));
    }

    [TestMethod]
    public async Task UpdateGroupAsync_ShouldReturnUpdatedGroup()
    {
        // Arrange
        var group = new RssFeedGroup
        {
            Id = "1",
            Name = "Updated Name",
        };
        _mockHandler.SetupResponse("/v1/categories/1", TestDataFactory.CreateCategoryResponse(1, "Updated Name"));

        // Act
        var result = await _client.UpdateGroupAsync(group);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("1", result.Id);
        Assert.AreEqual("Updated Name", result.Name);
    }

    [TestMethod]
    public async Task DeleteGroupAsync_ShouldReturnTrue()
    {
        // Arrange
        var group = TestDataFactory.CreateTestGroup();
        _mockHandler.SetupResponse("/v1/categories/1", HttpStatusCode.NoContent);

        // Act
        var result = await _client.DeleteGroupAsync(group);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteGroupAsync_WithServerError_ShouldReturnFalse()
    {
        // Arrange
        var group = TestDataFactory.CreateTestGroup("999");
        _mockHandler.SetupErrorResponse("/v1/categories/999", HttpStatusCode.NotFound, "Category not found");

        // Act
        var result = await _client.DeleteGroupAsync(group);

        // Assert
        Assert.IsFalse(result);
    }
}

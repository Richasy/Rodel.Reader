// Copyright (c) Richasy. All rights reserved.

namespace RssSource.NewsBlur.Test.UnitTests;

/// <summary>
/// NewsBlurClient 分组管理单元测试.
/// </summary>
[TestClass]
public sealed class NewsBlurClientGroupTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private NewsBlurClientOptions _options = null!;
    private NewsBlurClient _client = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new NewsBlurClient(_options, _httpClient);

        // 模拟登录成功
        _mockHandler.SetupResponse("/api/login", TestDataFactory.CreateLoginSuccessResponse());
        await _client.SignInAsync();
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
        var group = new RssFeedGroup { Name = "New Folder" };
        _mockHandler.SetupResponse("/reader/add_folder", TestDataFactory.CreateOperationSuccessResponse());

        // Act
        var result = await _client.AddGroupAsync(group);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("New Folder", result.Id);
        Assert.AreEqual("New Folder", result.Name);
    }

    [TestMethod]
    public async Task AddGroupAsync_WithServerError_ShouldReturnNull()
    {
        // Arrange
        var group = new RssFeedGroup { Name = "New Folder" };
        _mockHandler.SetupErrorResponse("/reader/add_folder", HttpStatusCode.InternalServerError, "Server error");

        // Act
        var result = await _client.AddGroupAsync(group);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task UpdateGroupAsync_ShouldReturnUpdatedGroup()
    {
        // Arrange
        var group = new RssFeedGroup { Id = "OldName", Name = "New Name" };
        _mockHandler.SetupResponse("/reader/rename_folder", TestDataFactory.CreateOperationSuccessResponse());

        // Act
        var result = await _client.UpdateGroupAsync(group);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("New Name", result.Id);
        Assert.AreEqual("New Name", result.Name);
    }

    [TestMethod]
    public async Task UpdateGroupAsync_WithServerError_ShouldReturnNull()
    {
        // Arrange
        var group = new RssFeedGroup { Id = "OldName", Name = "New Name" };
        _mockHandler.SetupErrorResponse("/reader/rename_folder", HttpStatusCode.InternalServerError, "Server error");

        // Act
        var result = await _client.UpdateGroupAsync(group);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task DeleteGroupAsync_ShouldSucceed()
    {
        // Arrange
        var group = TestDataFactory.CreateTestGroup();
        _mockHandler.SetupResponse("/reader/delete_folder", TestDataFactory.CreateOperationSuccessResponse());

        // Act
        var result = await _client.DeleteGroupAsync(group);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteGroupAsync_WithServerError_ShouldReturnFalse()
    {
        // Arrange
        var group = TestDataFactory.CreateTestGroup();
        _mockHandler.SetupErrorResponse("/reader/delete_folder", HttpStatusCode.InternalServerError, "Server error");

        // Act
        var result = await _client.DeleteGroupAsync(group);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AddGroupAsync_WithNullGroup_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.AddGroupAsync(null!));
    }

    [TestMethod]
    public async Task UpdateGroupAsync_WithNullGroup_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.UpdateGroupAsync(null!));
    }

    [TestMethod]
    public async Task DeleteGroupAsync_WithNullGroup_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.DeleteGroupAsync(null!));
    }
}

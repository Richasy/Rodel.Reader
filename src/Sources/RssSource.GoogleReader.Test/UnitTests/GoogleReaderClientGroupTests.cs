// Copyright (c) Richasy. All rights reserved.

namespace RssSource.GoogleReader.Test.UnitTests;

/// <summary>
/// GoogleReaderClient 分组管理单元测试.
/// </summary>
[TestClass]
public sealed class GoogleReaderClientGroupTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private GoogleReaderClientOptions _options = null!;
    private GoogleReaderClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new GoogleReaderClient(_options, _httpClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public void AddGroupAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var group = new RssFeedGroup { Name = "新分组" };

        // Act & Assert
        Assert.ThrowsExactly<NotSupportedException>(() => _client.AddGroupAsync(group).GetAwaiter().GetResult());
    }

    [TestMethod]
    public async Task UpdateGroupAsync_ShouldReturnUpdatedGroup()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/rename-tag", "OK");
        var group = TestDataFactory.TechGroup.Clone();
        group.Name = "科技新闻";

        // Act
        var result = await _client.UpdateGroupAsync(group);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("user/-/label/科技新闻", result.Id);
        Assert.AreEqual("科技新闻", result.Name);
    }

    [TestMethod]
    public async Task UpdateGroupAsync_WithFailure_ShouldReturnNull()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/rename-tag", "ERROR");
        var group = TestDataFactory.TechGroup.Clone();
        group.Name = "科技新闻";

        // Act
        var result = await _client.UpdateGroupAsync(group);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task DeleteGroupAsync_ShouldReturnTrue()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/disable-tag", "OK");

        // Act
        var result = await _client.DeleteGroupAsync(TestDataFactory.TechGroup);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteGroupAsync_WithFailure_ShouldReturnFalse()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/disable-tag", "ERROR");

        // Act
        var result = await _client.DeleteGroupAsync(TestDataFactory.TechGroup);

        // Assert
        Assert.IsFalse(result);
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

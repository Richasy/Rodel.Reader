// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Inoreader.Test.UnitTests;

/// <summary>
/// InoreaderClient 分组操作单元测试.
/// </summary>
[TestClass]
public sealed class InoreaderClientGroupTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private InoreaderClientOptions _options = null!;
    private InoreaderClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new InoreaderClient(_options, _httpClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public async Task AddGroupAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var group = new RssFeedGroup
        {
            Name = "新分组",
        };

        // Act & Assert
        await Assert.ThrowsExactlyAsync<NotSupportedException>(
            () => _client.AddGroupAsync(group));
    }

    [TestMethod]
    public async Task UpdateGroupAsync_ShouldReturnNewGroupWithUpdatedId()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/rename-tag", "OK");

        var group = TestDataFactory.TechGroup;
        group.Name = "新科技";

        // Act
        var result = await _client.UpdateGroupAsync(group);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("user/-/label/新科技", result.Id);
        Assert.AreEqual("新科技", result.Name);
    }

    [TestMethod]
    public async Task UpdateGroupAsync_ShouldSendCorrectRequest()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/rename-tag", "OK");

        var group = TestDataFactory.TechGroup;
        group.Name = "更新后的名称";

        // Act
        await _client.UpdateGroupAsync(group);

        // Assert
        var request = _mockHandler.Requests.First(r => r.RequestUri!.PathAndQuery.Contains("/rename-tag"));
        Assert.AreEqual(HttpMethod.Post, request.Method);

        var content = await request.Content!.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("s=user%2F-%2Flabel%2F") || content.Contains("s=user/-/label/"));
        Assert.IsTrue(content.Contains("dest=user%2F-%2Flabel%2F") || content.Contains("dest=user/-/label/"));
    }

    [TestMethod]
    public async Task UpdateGroupAsync_WhenFailed_ShouldReturnNull()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/rename-tag", "ERROR");

        var group = TestDataFactory.TechGroup;
        group.Name = "新名称";

        // Act
        var result = await _client.UpdateGroupAsync(group);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task DeleteGroupAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/disable-tag", "OK");

        // Act
        var result = await _client.DeleteGroupAsync(TestDataFactory.TechGroup);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteGroupAsync_ShouldSendCorrectRequest()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/disable-tag", "OK");

        // Act
        await _client.DeleteGroupAsync(TestDataFactory.TechGroup);

        // Assert
        var request = _mockHandler.Requests.First(r => r.RequestUri!.PathAndQuery.Contains("/disable-tag"));
        Assert.AreEqual(HttpMethod.Post, request.Method);

        var content = await request.Content!.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("s=user%2F-%2Flabel%2F") || content.Contains("s=user/-/label/"));
    }

    [TestMethod]
    public async Task DeleteGroupAsync_WhenFailed_ShouldReturnFalse()
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

// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Feedbin.Test.UnitTests;

/// <summary>
/// FeedbinClient 分组/标签管理单元测试.
/// </summary>
[TestClass]
public sealed class FeedbinClientGroupTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private FeedbinClientOptions _options = null!;
    private FeedbinClient _client = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _mockHandler.SetupTextResponse("/authentication.json", "{}", HttpStatusCode.OK);
        _client = new FeedbinClient(_options, _httpClient);
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
    public async Task AddGroupAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var group = TestDataFactory.TechGroup;

        // Act & Assert
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => _client.AddGroupAsync(group));
    }

    [TestMethod]
    public async Task UpdateGroupAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var group = TestDataFactory.TechGroup;

        // Act & Assert
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => _client.UpdateGroupAsync(group));
    }

    [TestMethod]
    public async Task DeleteGroupAsync_ShouldDeleteAllTaggingsForGroup()
    {
        // Arrange
        var group = TestDataFactory.TechGroup;
        _mockHandler.SetupTextResponse("/taggings.json", TestDataFactory.CreateTaggingsListJson());
        _mockHandler.SetupResponse("/taggings/1.json", _ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));
        _mockHandler.SetupResponse("/taggings/2.json", _ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));

        // Act
        var result = await _client.DeleteGroupAsync(group);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteGroupAsync_WithEmptyGroup_ShouldReturnTrue()
    {
        // Arrange
        var group = new RssFeedGroup { Id = "不存在的分组", Name = "不存在的分组" };
        _mockHandler.SetupTextResponse("/taggings.json", TestDataFactory.CreateTaggingsListJson());

        // Act
        var result = await _client.DeleteGroupAsync(group);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteGroupAsync_WithNullGroup_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.DeleteGroupAsync(null!));
    }

    [TestMethod]
    public async Task MarkGroupAsReadAsync_ShouldMarkAllGroupFeedsAsRead()
    {
        // Arrange
        var group = TestDataFactory.TechGroup;
        _mockHandler.SetupTextResponse("/subscriptions.json", TestDataFactory.CreateSubscriptionListJson());
        _mockHandler.SetupTextResponse("/taggings.json", TestDataFactory.CreateTaggingsListJson());
        _mockHandler.SetupTextResponse("/unread_entries.json", TestDataFactory.CreateUnreadEntriesJson());
        _mockHandler.SetupTextResponse("/feeds/123/entries.json", TestDataFactory.CreateEntriesListJson());
        _mockHandler.SetupTextResponse("/feeds/124/entries.json", "[]");

        _mockHandler.SetupResponse("/unread_entries.json", req =>
        {
            if (req.Method == HttpMethod.Delete)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(TestDataFactory.CreateUnreadEntriesJson(), System.Text.Encoding.UTF8, "application/json"),
            };
        });

        // Act
        var result = await _client.MarkGroupAsReadAsync(group);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task MarkGroupAsReadAsync_WithEmptyGroup_ShouldReturnTrue()
    {
        // Arrange
        var group = new RssFeedGroup { Id = "空分组", Name = "空分组" };
        _mockHandler.SetupTextResponse("/subscriptions.json", TestDataFactory.CreateSubscriptionListJson());
        _mockHandler.SetupTextResponse("/taggings.json", TestDataFactory.CreateEmptyTaggingsListJson());

        // Act
        var result = await _client.MarkGroupAsReadAsync(group);

        // Assert
        Assert.IsTrue(result);
    }
}

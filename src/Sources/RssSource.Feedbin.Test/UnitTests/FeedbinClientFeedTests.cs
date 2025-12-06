// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Feedbin.Test.UnitTests;

/// <summary>
/// FeedbinClient 订阅源管理单元测试.
/// </summary>
[TestClass]
public sealed class FeedbinClientFeedTests
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
    public async Task GetFeedListAsync_ShouldReturnGroupsAndFeeds()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscriptions.json", TestDataFactory.CreateSubscriptionListJson());
        _mockHandler.SetupTextResponse("/taggings.json", TestDataFactory.CreateTaggingsListJson());

        // Act
        var (groups, feeds) = await _client.GetFeedListAsync();

        // Assert
        Assert.IsNotNull(groups);
        Assert.IsNotNull(feeds);
        Assert.AreEqual(2, groups.Count); // 科技 和 开发
        Assert.AreEqual(3, feeds.Count);

        // 验证分组
        Assert.IsTrue(groups.Any(g => g.Name == "科技"));
        Assert.IsTrue(groups.Any(g => g.Name == "开发"));

        // 验证订阅源
        var ithome = feeds.FirstOrDefault(f => f.Name == "IT之家");
        Assert.IsNotNull(ithome);
        Assert.AreEqual("123", ithome.Id);
        Assert.AreEqual("1001", ithome.Comment); // subscription_id
        Assert.IsTrue(ithome.GetGroupIdList().Contains("科技"));
    }

    [TestMethod]
    public async Task GetFeedListAsync_WithEmptyTaggings_ShouldReturnFeedsWithoutGroups()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscriptions.json", TestDataFactory.CreateSubscriptionListJson());
        _mockHandler.SetupTextResponse("/taggings.json", TestDataFactory.CreateEmptyTaggingsListJson());

        // Act
        var (groups, feeds) = await _client.GetFeedListAsync();

        // Assert
        Assert.AreEqual(0, groups.Count);
        Assert.AreEqual(3, feeds.Count);
        Assert.IsTrue(feeds.All(f => f.GetGroupIdList().Count == 0));
    }

    [TestMethod]
    public async Task GetFeedListAsync_WithoutAuth_ShouldThrowException()
    {
        // Arrange
        await _client.SignOutAsync();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => _client.GetFeedListAsync());
    }

    [TestMethod]
    public async Task AddFeedAsync_ShouldCreateSubscriptionAndReturnFeed()
    {
        // Arrange
        var newFeed = new RssFeed
        {
            Name = "New Feed",
            Url = "https://example.com/feed.xml",
        };

        _mockHandler.SetupResponse("/subscriptions.json", req =>
        {
            if (req.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(TestDataFactory.CreateSubscriptionJson(), System.Text.Encoding.UTF8, "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        // Act
        var result = await _client.AddFeedAsync(newFeed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("126", result.Id);
        Assert.AreEqual("1004", result.Comment);
        Assert.AreEqual("https://example.com/feed.xml", result.Url);
    }

    [TestMethod]
    public async Task AddFeedAsync_WithNullFeed_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.AddFeedAsync(null!));
    }

    [TestMethod]
    public async Task DeleteFeedAsync_ShouldReturnTrueOnSuccess()
    {
        // Arrange
        var feed = TestDataFactory.IthomeFeed;
        _mockHandler.SetupResponse("/subscriptions/1001.json", _ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));

        // Act
        var result = await _client.DeleteFeedAsync(feed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteFeedAsync_WithInvalidSubscriptionId_ShouldReturnFalse()
    {
        // Arrange
        var feed = new RssFeed
        {
            Id = "123",
            Name = "Test",
            Comment = null, // 没有 subscription_id
        };

        // Act
        var result = await _client.DeleteFeedAsync(feed);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task UpdateFeedAsync_ShouldUpdateTitleAndGroups()
    {
        // Arrange
        var oldFeed = new RssFeed
        {
            Id = "123",
            Name = "Old Name",
            Comment = "1001",
            GroupIds = "科技",
        };

        var newFeed = new RssFeed
        {
            Id = "123",
            Name = "New Name",
            Comment = "1001",
            GroupIds = "开发",
        };

        _mockHandler.SetupResponse("/subscriptions/1001.json", _ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json"),
            });
        _mockHandler.SetupTextResponse("/taggings.json", TestDataFactory.CreateTaggingsListJson());
        _mockHandler.SetupResponse("/taggings/1.json", _ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));
        _mockHandler.SetupResponse("/taggings.json", req =>
        {
            if (req.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(TestDataFactory.CreateTaggingJson(), System.Text.Encoding.UTF8, "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(TestDataFactory.CreateTaggingsListJson(), System.Text.Encoding.UTF8, "application/json"),
            };
        });

        // Act
        var result = await _client.UpdateFeedAsync(newFeed, oldFeed);

        // Assert
        Assert.IsTrue(result);
    }
}

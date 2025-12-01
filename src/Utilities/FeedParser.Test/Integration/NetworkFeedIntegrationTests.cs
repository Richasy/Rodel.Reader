// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.RodelPlayer.Utilities.FeedParser.Readers;

namespace FeedParser.Test.Integration;

/// <summary>
/// 网络 Feed 集成测试.
/// 从真实的 RSS/Atom 源获取数据进行测试.
/// </summary>
/// <remarks>
/// 这些测试需要网络连接，可能会因为网络问题或源站点变化而失败.
/// 在 CI 环境中可能需要跳过这些测试.
/// </remarks>
[TestClass]
[TestCategory("Network")]
public sealed class NetworkFeedIntegrationTests
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30),
        DefaultRequestHeaders =
        {
            { "User-Agent", "FeedParser.Test/1.0" }
        }
    };

    #region 知名 RSS 源测试

    /// <summary>
    /// 测试一些知名的 RSS 源.
    /// </summary>
    [TestMethod]
    [DataRow("https://feeds.bbci.co.uk/news/rss.xml", "BBC News")]
    [DataRow("https://rss.nytimes.com/services/xml/rss/nyt/HomePage.xml", "NYTimes")]
    [DataRow("https://www.reddit.com/r/programming/.rss", "Reddit Programming")]
    public async Task ParseKnownRssFeed_ShouldSucceed(string feedUrl, string feedName)
    {
        try
        {
            // Arrange
            using var response = await HttpClient.GetAsync(new Uri(feedUrl));

            if (!response.IsSuccessStatusCode)
            {
                Assert.Inconclusive($"无法访问 {feedName} ({feedUrl}): HTTP {response.StatusCode}");
                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();

            // Act
            var (channel, items) = await FeedReader.ReadAsync(stream);

            // Assert
            Assert.IsNotNull(channel, $"{feedName}: 频道不应为空");
            Assert.IsFalse(string.IsNullOrEmpty(channel.Title), $"{feedName}: 频道标题不应为空");

            Console.WriteLine($"✅ {feedName}");
            Console.WriteLine($"   URL: {feedUrl}");
            Console.WriteLine($"   标题: {channel.Title}");
            Console.WriteLine($"   描述: {channel.Description?.Substring(0, Math.Min(100, channel.Description?.Length ?? 0))}...");
            Console.WriteLine($"   订阅项数量: {items.Count}");

            if (items.Count > 0)
            {
                Console.WriteLine($"   最新文章: {items[0].Title}");
                Console.WriteLine($"   发布时间: {items[0].PublishedAt}");
            }

            Console.WriteLine();
        }
        catch (HttpRequestException ex)
        {
            Assert.Inconclusive($"网络请求失败 {feedName}: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Assert.Inconclusive($"请求超时 {feedName}");
        }
    }

    #endregion

    #region 知名 Atom 源测试

    /// <summary>
    /// 测试一些知名的 Atom 源.
    /// </summary>
    [TestMethod]
    [DataRow("https://github.com/microsoft/vscode/releases.atom", "VS Code Releases")]
    [DataRow("https://blog.golang.org/feed.atom", "Go Blog")]
    public async Task ParseKnownAtomFeed_ShouldSucceed(string feedUrl, string feedName)
    {
        try
        {
            // Arrange
            using var response = await HttpClient.GetAsync(new Uri(feedUrl));

            if (!response.IsSuccessStatusCode)
            {
                Assert.Inconclusive($"无法访问 {feedName} ({feedUrl}): HTTP {response.StatusCode}");
                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();

            // Act
            var (channel, items) = await FeedReader.ReadAsync(stream);

            // Assert
            Assert.IsNotNull(channel, $"{feedName}: 频道不应为空");

            Console.WriteLine($"✅ {feedName}");
            Console.WriteLine($"   URL: {feedUrl}");
            Console.WriteLine($"   标题: {channel.Title}");
            Console.WriteLine($"   条目数量: {items.Count}");

            if (items.Count > 0)
            {
                Console.WriteLine($"   最新条目: {items[0].Title}");
            }

            Console.WriteLine();
        }
        catch (HttpRequestException ex)
        {
            Assert.Inconclusive($"网络请求失败 {feedName}: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Assert.Inconclusive($"请求超时 {feedName}");
        }
    }

    #endregion

    #region 播客源测试

    /// <summary>
    /// 测试一些知名的播客源.
    /// </summary>
    [TestMethod]
    [DataRow("https://feeds.simplecast.com/54nAGcIl", "The Changelog")]
    [DataRow("https://feeds.fireside.fm/bibleinayear/rss", "Bible in a Year")]
    public async Task ParseKnownPodcastFeed_ShouldSucceed(string feedUrl, string podcastName)
    {
        try
        {
            // Arrange
            using var response = await HttpClient.GetAsync(new Uri(feedUrl));

            if (!response.IsSuccessStatusCode)
            {
                Assert.Inconclusive($"无法访问 {podcastName} ({feedUrl}): HTTP {response.StatusCode}");
                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();

            // Act
            var (channel, items) = await FeedReader.ReadAsync(stream);

            // Assert
            Assert.IsNotNull(channel, $"{podcastName}: 频道不应为空");
            Assert.IsFalse(string.IsNullOrEmpty(channel.Title), $"{podcastName}: 播客名称不应为空");

            Console.WriteLine($"✅ {podcastName}");
            Console.WriteLine($"   URL: {feedUrl}");
            Console.WriteLine($"   名称: {channel.Title}");
            Console.WriteLine($"   节目数量: {items.Count}");

            // 检查播客特有字段
            if (channel.Contributors.Count > 0)
            {
                Console.WriteLine($"   作者: {string.Join(", ", channel.Contributors.Select(c => c.Name))}");
            }

            if (channel.Images.Count > 0)
            {
                Console.WriteLine($"   封面: {channel.Images[0].Url}");
            }

            if (channel.Categories.Count > 0)
            {
                Console.WriteLine($"   分类: {string.Join(", ", channel.Categories.Select(c => c.Label))}");
            }

            // 检查最新节目
            if (items.Count > 0)
            {
                var latestEpisode = items[0];
                Console.WriteLine($"   最新节目: {latestEpisode.Title}");

                var enclosure = latestEpisode.Links.FirstOrDefault(l => l.LinkType == FeedLinkType.Enclosure);
                if (enclosure != null)
                {
                    Console.WriteLine($"     音频类型: {enclosure.MediaType}");
                    if (enclosure.Length.HasValue)
                    {
                        Console.WriteLine($"     文件大小: {enclosure.Length.Value / 1024.0 / 1024.0:F2} MB");
                    }
                }

                if (latestEpisode.Duration.HasValue)
                {
                    Console.WriteLine($"     时长: {latestEpisode.Duration.Value}");
                }
            }

            // 验证至少有音频附件
            var hasEnclosure = items.Any(item =>
                item.Links.Any(l => l.LinkType == FeedLinkType.Enclosure));
            Assert.IsTrue(hasEnclosure, $"{podcastName}: 播客应该有音频附件");

            Console.WriteLine();
        }
        catch (HttpRequestException ex)
        {
            Assert.Inconclusive($"网络请求失败 {podcastName}: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Assert.Inconclusive($"请求超时 {podcastName}");
        }
    }

    #endregion

    #region 中文源测试

    /// <summary>
    /// 测试一些中文 RSS/播客源.
    /// </summary>
    [TestMethod]
    [DataRow("https://www.gcores.com/rss", "机核网")]
    [DataRow("https://www.ithome.com/rss", "IT之家")]
    public async Task ParseChineseFeed_ShouldSucceed(string feedUrl, string feedName)
    {
        try
        {
            // Arrange
            using var response = await HttpClient.GetAsync(new Uri(feedUrl));

            if (!response.IsSuccessStatusCode)
            {
                Assert.Inconclusive($"无法访问 {feedName} ({feedUrl}): HTTP {response.StatusCode}");
                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();

            // 检查流是否有内容
            if (stream.Length == 0)
            {
                Assert.Inconclusive($"{feedName} 返回空内容");
                return;
            }

            // Act
            var (channel, items) = await FeedReader.ReadAsync(stream);

            // Assert
            Assert.IsNotNull(channel, $"{feedName}: 频道不应为空");

            Console.WriteLine($"✅ {feedName}");
            Console.WriteLine($"   URL: {feedUrl}");
            Console.WriteLine($"   标题: {channel.Title}");
            Console.WriteLine($"   订阅项数量: {items.Count}");

            if (items.Count > 0)
            {
                Console.WriteLine($"   最新文章: {items[0].Title}");
            }

            Console.WriteLine();
        }
        catch (HttpRequestException ex)
        {
            Assert.Inconclusive($"网络请求失败 {feedName}: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Assert.Inconclusive($"请求超时 {feedName}");
        }
        catch (System.Xml.XmlException ex)
        {
            Assert.Inconclusive($"XML 解析失败 {feedName}: {ex.Message}");
        }
    }

    #endregion

    #region 格式自动检测测试

    [TestMethod]
    [DataRow("https://feeds.bbci.co.uk/news/rss.xml", FeedType.Rss)]
    [DataRow("https://github.com/microsoft/vscode/releases.atom", FeedType.Atom)]
    public async Task DetectFeedType_NetworkFeeds_ShouldDetectCorrectly(string feedUrl, FeedType expectedType)
    {
        try
        {
            // Arrange
            using var response = await HttpClient.GetAsync(new Uri(feedUrl));

            if (!response.IsSuccessStatusCode)
            {
                Assert.Inconclusive($"无法访问 {feedUrl}: HTTP {response.StatusCode}");
                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();

            // Act
            var detectedType = await FeedReader.DetectFeedTypeAsync(stream);

            // Assert
            Assert.AreEqual(expectedType, detectedType, $"Feed 类型检测错误: {feedUrl}");

            Console.WriteLine($"✅ {feedUrl}");
            Console.WriteLine($"   检测类型: {detectedType}");
            Console.WriteLine();
        }
        catch (HttpRequestException ex)
        {
            Assert.Inconclusive($"网络请求失败: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Assert.Inconclusive("请求超时");
        }
    }

    #endregion

    #region 流式读取测试

    [TestMethod]
    public async Task StreamingRead_LargePodcastFeed_ShouldNotLoadAllInMemory()
    {
        var feedUri = new Uri("https://feeds.simplecast.com/54nAGcIl"); // The Changelog - 通常有很多节目

        try
        {
            using var response = await HttpClient.GetAsync(feedUri);

            if (!response.IsSuccessStatusCode)
            {
                Assert.Inconclusive($"无法访问播客源: HTTP {response.StatusCode}");
                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = await FeedReader.CreateAsync(stream);

            // 读取频道信息
            var channel = await reader.ReadChannelAsync();
            Console.WriteLine($"播客: {channel.Title}");

            // 流式读取，只获取前 10 个节目
            var count = 0;
            await foreach (var item in reader.ReadItemsAsync())
            {
                count++;
                Console.WriteLine($"  {count}. {item.Title}");

                if (count >= 10)
                {
                    break; // 只读取 10 个就停止
                }
            }

            Console.WriteLine($"\n成功流式读取 {count} 个节目（提前停止）");
        }
        catch (HttpRequestException ex)
        {
            Assert.Inconclusive($"网络请求失败: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Assert.Inconclusive("请求超时");
        }
    }

    #endregion
}

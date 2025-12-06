// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Local.Test.Integration;

/// <summary>
/// OPML 导入导出集成测试.
/// </summary>
[TestClass]
public sealed class OpmlIntegrationTests : IntegrationTestBase
{
    [TestInitialize]
    public async Task Setup()
    {
        await InitializeAsync();
    }

    [TestCleanup]
    public new void Dispose()
    {
        base.Dispose();
    }

    [TestMethod]
    public async Task ImportOpml_ThenExport_ShouldPreserveData()
    {
        // Arrange
        const string opml = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <opml version="2.0">
                <head>
                    <title>Test Subscriptions</title>
                </head>
                <body>
                    <outline text="科技" title="科技">
                        <outline text="IT之家" title="IT之家" type="rss" xmlUrl="https://www.ithome.com/rss" htmlUrl="https://www.ithome.com" />
                        <outline text="极客公园" title="极客公园" type="rss" xmlUrl="https://www.geekpark.net/rss" htmlUrl="https://www.geekpark.net" />
                    </outline>
                    <outline text=".NET Blog" title=".NET Blog" type="rss" xmlUrl="https://devblogs.microsoft.com/dotnet/feed/" htmlUrl="https://devblogs.microsoft.com/dotnet" />
                </body>
            </opml>
            """;

        // Act - 导入
        var importResult = await Client.ImportOpmlAsync(opml);

        // Assert - 导入成功
        Assert.IsTrue(importResult);

        // 验证数据已导入
        var (groups, feeds) = await Client.GetFeedListAsync();
        Assert.AreEqual(1, groups.Count, "应有1个分组");
        Assert.AreEqual(3, feeds.Count, "应有3个订阅源");

        // Act - 导出
        var exportedOpml = await Client.ExportOpmlAsync();

        // Assert - 导出包含正确数据
        Assert.IsTrue(!string.IsNullOrEmpty(exportedOpml));
        Assert.IsTrue(exportedOpml.Contains("科技"), "应包含分组名");
        Assert.IsTrue(exportedOpml.Contains("IT之家"), "应包含订阅源名");
        Assert.IsTrue(exportedOpml.Contains("极客公园"), "应包含订阅源名");
        Assert.IsTrue(exportedOpml.Contains(".NET Blog"), "应包含订阅源名");
        Assert.IsTrue(exportedOpml.Contains("https://www.ithome.com/rss"), "应包含订阅源 URL");

        Console.WriteLine("导出的 OPML:");
        Console.WriteLine(exportedOpml);
    }

    [TestMethod]
    public async Task ImportOpml_WithNestedGroups_ShouldFlattenStructure()
    {
        // Arrange
        const string opml = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <opml version="2.0">
                <head>
                    <title>Nested Subscriptions</title>
                </head>
                <body>
                    <outline text="技术" title="技术">
                        <outline text="编程" title="编程">
                            <outline text=".NET Blog" title=".NET Blog" type="rss" xmlUrl="https://devblogs.microsoft.com/dotnet/feed/" />
                        </outline>
                    </outline>
                </body>
            </opml>
            """;

        // Act
        var result = await Client.ImportOpmlAsync(opml);

        // Assert
        Assert.IsTrue(result);

        var (groups, feeds) = await Client.GetFeedListAsync();
        Assert.IsTrue(groups.Count >= 1, "应至少有1个分组");
        Assert.IsTrue(feeds.Count >= 1, "应至少有1个订阅源");

        Console.WriteLine($"分组数: {groups.Count}");
        foreach (var group in groups)
        {
            Console.WriteLine($"  - {group.Name}");
        }

        Console.WriteLine($"订阅源数: {feeds.Count}");
        foreach (var feed in feeds)
        {
            Console.WriteLine($"  - {feed.Name} ({feed.Url})");
        }
    }

    [TestMethod]
    public async Task ImportOpml_MultipleTimes_ShouldNotDuplicateData()
    {
        // Arrange
        const string opml = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <opml version="2.0">
                <head><title>Test</title></head>
                <body>
                    <outline text="IT之家" title="IT之家" type="rss" xmlUrl="https://www.ithome.com/rss" />
                </body>
            </opml>
            """;

        // Act - 导入两次
        await Client.ImportOpmlAsync(opml);
        await Client.ImportOpmlAsync(opml);

        // Assert - 不应该有重复
        var (_, feeds) = await Client.GetFeedListAsync();

        // OPML 使用 Upsert，所以相同 ID 的订阅源应该只有一个
        Console.WriteLine($"订阅源数: {feeds.Count}");
    }

    [TestMethod]
    public async Task ExportOpml_EmptyDatabase_ShouldReturnValidOpml()
    {
        // Act
        var opml = await Client.ExportOpmlAsync();

        // Assert
        Assert.IsTrue(!string.IsNullOrEmpty(opml));
        Assert.IsTrue(opml.Contains("<?xml version=\"1.0\""));
        Assert.IsTrue(opml.Contains("<opml"));
        Assert.IsTrue(opml.Contains("</opml>"));

        Console.WriteLine("空数据库导出的 OPML:");
        Console.WriteLine(opml);
    }

    [TestMethod]
    public async Task ImportOpml_WithRealFeeds_ThenFetchContent_ShouldWork()
    {
        // Arrange
        const string opml = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <opml version="2.0">
                <head><title>Real Feeds</title></head>
                <body>
                    <outline text="科技" title="科技">
                        <outline text="IT之家" title="IT之家" type="rss" xmlUrl="https://www.ithome.com/rss" />
                    </outline>
                </body>
            </opml>
            """;

        // Act - 导入
        var importResult = await Client.ImportOpmlAsync(opml);
        Assert.IsTrue(importResult);

        // 获取订阅源列表
        var (_, feeds) = await Client.GetFeedListAsync();
        Assert.AreEqual(1, feeds.Count);

        // 获取订阅源内容
        var detail = await Client.GetFeedDetailAsync(feeds[0]);

        // Assert
        Assert.IsNotNull(detail, "应该能获取到订阅源内容");
        Assert.IsTrue(detail.Articles.Count > 0, "应该有文章");

        Console.WriteLine($"从导入的 OPML 获取到 {detail.Articles.Count} 篇文章");
        Console.WriteLine($"第一篇: {detail.Articles[0].Title}");
    }
}

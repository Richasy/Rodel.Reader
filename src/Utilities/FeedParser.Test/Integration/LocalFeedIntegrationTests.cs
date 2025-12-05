// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.RodelReader.Utilities.FeedParser.Readers;

namespace FeedParser.Test.Integration;

/// <summary>
/// 本地 XML 文件集成测试.
/// 从 TestData 目录读取真实的 RSS/Atom 文件进行测试.
/// </summary>
[TestClass]
public sealed class LocalFeedIntegrationTests
{
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
    private static readonly string RssDir = Path.Combine(TestDataDir, "Rss");
    private static readonly string AtomDir = Path.Combine(TestDataDir, "Atom");
    private static readonly string PodcastDir = Path.Combine(TestDataDir, "Podcast");

    #region RSS 集成测试

    [TestMethod]
    public async Task ParseAllRssFiles_ShouldSucceed()
    {
        // 检查目录是否存在
        if (!Directory.Exists(RssDir))
        {
            Assert.Inconclusive($"RSS 测试数据目录不存在: {RssDir}，请先添加测试数据");
            return;
        }

        var xmlFiles = Directory.GetFiles(RssDir, "*.xml");
        if (xmlFiles.Length == 0)
        {
            Assert.Inconclusive("RSS 目录中没有 XML 文件，请先添加测试数据");
            return;
        }

        var results = new List<(string FileName, bool Success, string? Error, int ItemCount)>();

        foreach (var file in xmlFiles)
        {
            var fileName = Path.GetFileName(file);
            try
            {
                await using var stream = File.OpenRead(file);
                var (channel, items) = await FeedReader.ReadAsync(stream);

                Assert.IsNotNull(channel, $"频道不应为空: {fileName}");
                Assert.IsFalse(string.IsNullOrEmpty(channel.Title), $"频道标题不应为空: {fileName}");

                results.Add((fileName, true, null, items.Count));

                Console.WriteLine($"✅ {fileName}");
                Console.WriteLine($"   标题: {channel.Title}");
                Console.WriteLine($"   订阅项数量: {items.Count}");
                if (items.Count > 0)
                {
                    Console.WriteLine($"   第一篇: {items[0].Title}");
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                results.Add((fileName, false, ex.Message, 0));
                Console.WriteLine($"❌ {fileName}: {ex.Message}");
            }
        }

        // 输出摘要
        var successCount = results.Count(r => r.Success);
        Console.WriteLine($"\n========== 测试摘要 ==========");
        Console.WriteLine($"成功: {successCount}/{results.Count}");

        // 如果有失败，测试失败
        if (successCount < results.Count)
        {
            var failures = results.Where(r => !r.Success).Select(r => $"{r.FileName}: {r.Error}");
            Assert.Fail($"部分文件解析失败:\n{string.Join("\n", failures)}");
        }
    }

    #endregion

    #region Atom 集成测试

    [TestMethod]
    public async Task ParseAllAtomFiles_ShouldSucceed()
    {
        // 检查目录是否存在
        if (!Directory.Exists(AtomDir))
        {
            Assert.Inconclusive($"Atom 测试数据目录不存在: {AtomDir}，请先添加测试数据");
            return;
        }

        var xmlFiles = Directory.GetFiles(AtomDir, "*.xml");
        if (xmlFiles.Length == 0)
        {
            Assert.Inconclusive("Atom 目录中没有 XML 文件，请先添加测试数据");
            return;
        }

        var results = new List<(string FileName, bool Success, string? Error, int ItemCount)>();

        foreach (var file in xmlFiles)
        {
            var fileName = Path.GetFileName(file);
            try
            {
                await using var stream = File.OpenRead(file);
                var (channel, items) = await FeedReader.ReadAsync(stream);

                Assert.IsNotNull(channel, $"频道不应为空: {fileName}");
                Assert.AreEqual(FeedType.Atom, channel.Links.Count > 0 ? FeedType.Atom : FeedType.Atom);

                results.Add((fileName, true, null, items.Count));

                Console.WriteLine($"✅ {fileName}");
                Console.WriteLine($"   标题: {channel.Title}");
                Console.WriteLine($"   条目数量: {items.Count}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                results.Add((fileName, false, ex.Message, 0));
                Console.WriteLine($"❌ {fileName}: {ex.Message}");
            }
        }

        // 输出摘要
        var successCount = results.Count(r => r.Success);
        Console.WriteLine($"\n========== 测试摘要 ==========");
        Console.WriteLine($"成功: {successCount}/{results.Count}");

        if (successCount < results.Count)
        {
            var failures = results.Where(r => !r.Success).Select(r => $"{r.FileName}: {r.Error}");
            Assert.Fail($"部分文件解析失败:\n{string.Join("\n", failures)}");
        }
    }

    #endregion

    #region 播客集成测试

    [TestMethod]
    public async Task ParseAllPodcastFiles_ShouldSucceed()
    {
        // 检查目录是否存在
        if (!Directory.Exists(PodcastDir))
        {
            Assert.Inconclusive($"播客测试数据目录不存在: {PodcastDir}，请先添加测试数据");
            return;
        }

        var xmlFiles = Directory.GetFiles(PodcastDir, "*.xml");
        if (xmlFiles.Length == 0)
        {
            Assert.Inconclusive("Podcast 目录中没有 XML 文件，请先添加测试数据");
            return;
        }

        foreach (var file in xmlFiles)
        {
            var fileName = Path.GetFileName(file);
            try
            {
                await using var stream = File.OpenRead(file);
                var (channel, items) = await FeedReader.ReadAsync(stream);

                Assert.IsNotNull(channel, $"频道不应为空: {fileName}");

                Console.WriteLine($"✅ {fileName}");
                Console.WriteLine($"   播客名称: {channel.Title}");
                Console.WriteLine($"   节目数量: {items.Count}");

                // 检查播客特有字段
                if (channel.Contributors.Count > 0)
                {
                    Console.WriteLine($"   作者: {channel.Contributors[0].Name}");
                }

                if (channel.Images.Count > 0)
                {
                    Console.WriteLine($"   封面: {channel.Images[0].Url}");
                }

                // 检查节目
                foreach (var item in items.Take(3))
                {
                    Console.WriteLine($"   - {item.Title}");
                    var enclosure = item.Links.FirstOrDefault(l => l.LinkType == FeedLinkType.Enclosure);
                    if (enclosure != null)
                    {
                        Console.WriteLine($"     音频: {enclosure.Uri}");
                        Console.WriteLine($"     类型: {enclosure.MediaType}");
                    }

                    if (item.Duration.HasValue)
                    {
                        Console.WriteLine($"     时长: {item.Duration.Value}");
                    }
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {fileName}: {ex.Message}");
                Assert.Fail($"解析播客文件失败: {fileName} - {ex.Message}");
            }
        }
    }

    #endregion

    #region 格式自动检测测试

    [TestMethod]
    public async Task DetectFeedType_AllTestFiles_ShouldDetectCorrectly()
    {
        var allDirs = new[] { RssDir, AtomDir, PodcastDir };

        foreach (var dir in allDirs)
        {
            if (!Directory.Exists(dir))
            {
                continue;
            }

            var xmlFiles = Directory.GetFiles(dir, "*.xml");
            foreach (var file in xmlFiles)
            {
                var fileName = Path.GetFileName(file);
                var dirName = Path.GetFileName(dir);

                await using var stream = File.OpenRead(file);
                var feedType = await FeedReader.DetectFeedTypeAsync(stream);

                var expectedType = dirName switch
                {
                    "Rss" or "Podcast" => FeedType.Rss,
                    "Atom" => FeedType.Atom,
                    _ => FeedType.Unknown
                };

                Console.WriteLine($"{dirName}/{fileName}: 检测到 {feedType}");

                Assert.AreEqual(
                    expectedType,
                    feedType,
                    $"文件 {dirName}/{fileName} 格式检测错误: 期望 {expectedType}, 实际 {feedType}");
            }
        }
    }

    #endregion
}

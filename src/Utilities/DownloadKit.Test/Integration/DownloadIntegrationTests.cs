// Copyright (c) Richasy. All rights reserved.

namespace DownloadKit.Test.Integration;

/// <summary>
/// 下载集成测试.
/// </summary>
/// <remarks>
/// 这些测试需要网络连接，用于验证实际下载功能.
/// </remarks>
[TestClass]
[TestCategory("Integration")]
public sealed class DownloadIntegrationTests
{
    private string? _tempFilePath;

    /// <summary>
    /// 测试清理.
    /// </summary>
    [TestCleanup]
    public void Cleanup()
    {
        TestDataFactory.CleanupFile(_tempFilePath);
    }

    /// <summary>
    /// 测试从真实 URL 下载.
    /// </summary>
    /// <remarks>
    /// 使用 httpbin.org 作为测试服务器.
    /// </remarks>
    [TestMethod]
    public async Task DownloadAsync_RealUrl_ShouldSucceed()
    {
        // Arrange
        var testUrl = new Uri("https://httpbin.org/bytes/1024"); // 返回 1024 字节的随机数据
        _tempFilePath = TestDataFactory.GetTempFilePath();

        using var client = new DownloadClient();

        var progressReports = new List<DownloadProgress>();
        var progress = new Progress<DownloadProgress>(p =>
        {
            progressReports.Add(p);
            Console.WriteLine($"进度: {p.Percentage:F1}% - {p.GetFormattedSpeed()}");
        });

        // Act
        var result = await client.DownloadAsync(testUrl, _tempFilePath, progress: progress);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1024, result.TotalBytes);
        Assert.IsTrue(File.Exists(_tempFilePath));

        var fileInfo = new FileInfo(_tempFilePath);
        Assert.AreEqual(1024, fileInfo.Length);

        Console.WriteLine($"下载完成: {result.ElapsedTime}");
        Console.WriteLine($"平均速度: {result.AverageSpeed / 1024:F2} KB/s");
    }

    /// <summary>
    /// 测试下载带自定义请求头.
    /// </summary>
    [TestMethod]
    public async Task DownloadAsync_WithCustomHeaders_ShouldSucceed()
    {
        // Arrange
        var testUrl = new Uri("https://httpbin.org/headers");
        _tempFilePath = TestDataFactory.GetTempFilePath();

        using var client = new DownloadClient();

        var options = new DownloadOptions
        {
            Headers =
            {
                ["X-Custom-Header"] = "TestValue",
                ["Accept"] = "application/json",
            },
            UserAgent = "DownloadKit-Test/1.0",
        };

        // Act
        var result = await client.DownloadAsync(testUrl, _tempFilePath, options);

        // Assert
        Assert.IsTrue(result.IsSuccess);

        var content = await File.ReadAllTextAsync(_tempFilePath);
        Console.WriteLine(content);

        Assert.IsTrue(content.Contains("X-Custom-Header", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 测试下载取消.
    /// </summary>
    [TestMethod]
    public async Task DownloadAsync_CancelDuringDownload_ShouldCancel()
    {
        // Arrange
        var testUrl = new Uri("https://httpbin.org/bytes/1048576"); // 1MB
        _tempFilePath = TestDataFactory.GetTempFilePath();

        using var client = new DownloadClient();
        using var cts = new CancellationTokenSource();

        // 500ms 后取消
        cts.CancelAfter(500);

        // Act
        var result = await client.DownloadAsync(testUrl, _tempFilePath, cancellationToken: cts.Token);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(DownloadState.Canceled, result.State);
        Assert.IsFalse(File.Exists(_tempFilePath)); // 取消后应该清理部分文件

        Console.WriteLine($"取消前已下载: {result.TotalBytes} 字节");
    }

    /// <summary>
    /// 测试获取远程文件信息.
    /// </summary>
    [TestMethod]
    public async Task GetFileInfoAsync_RealUrl_ShouldReturnInfo()
    {
        // Arrange
        var testUrl = "https://httpbin.org/bytes/2048";

        using var client = new DownloadClient();

        // Act
        var info = await client.GetFileInfoAsync(testUrl);

        // Assert
        Console.WriteLine($"Content-Length: {info.ContentLength}");
        Console.WriteLine($"Content-Type: {info.ContentType}");
        Console.WriteLine($"Accept-Ranges: {info.AcceptRanges}");
        Console.WriteLine($"ETag: {info.ETag}");
    }

    /// <summary>
    /// 测试下载大文件并验证进度.
    /// </summary>
    [TestMethod]
    public async Task DownloadAsync_LargeFile_ShouldReportProgressCorrectly()
    {
        // Arrange
        var testUrl = new Uri("https://httpbin.org/bytes/102400"); // 100KB
        _tempFilePath = TestDataFactory.GetTempFilePath();

        using var client = new DownloadClient();

        var options = new DownloadOptions
        {
            ProgressThrottleMs = 50, // 更频繁的进度报告
        };

        var progressReports = new List<DownloadProgress>();
        var progress = new Progress<DownloadProgress>(p =>
        {
            progressReports.Add(p);
        });

        // Act
        var result = await client.DownloadAsync(testUrl, _tempFilePath, options, progress);

        // Assert
        Assert.IsTrue(result.IsSuccess);

        Console.WriteLine($"进度报告次数: {progressReports.Count}");
        foreach (var p in progressReports)
        {
            Console.WriteLine($"  {p.BytesReceived}/{p.TotalBytes} ({p.Percentage:F1}%) - {p.GetFormattedSpeed()} - {p.State}");
        }

        // 验证进度报告顺序
        for (var i = 1; i < progressReports.Count; i++)
        {
            Assert.IsTrue(progressReports[i].BytesReceived >= progressReports[i - 1].BytesReceived);
        }

        // 最后一个进度应该是完成状态
        var lastProgress = progressReports[^1];
        Assert.AreEqual(DownloadState.Completed, lastProgress.State);
    }
}

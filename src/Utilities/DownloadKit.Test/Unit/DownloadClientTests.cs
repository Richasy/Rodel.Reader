// Copyright (c) Richasy. All rights reserved.

using RichardSzalay.MockHttp;

namespace DownloadKit.Test.Unit;

/// <summary>
/// DownloadClient 单元测试.
/// </summary>
[TestClass]
public sealed class DownloadClientTests
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
    /// 测试下载成功.
    /// </summary>
    [TestMethod]
    public async Task DownloadAsync_Success_ShouldReturnSuccessResult()
    {
        // Arrange
        var testContent = TestDataFactory.CreateRandomBytes(1024);
        var testUrl = new Uri("http://test.example.com/file.bin");
        _tempFilePath = TestDataFactory.GetTempFilePath();

        var mockHttp = TestDataFactory.CreateMockHandler(testUrl.ToString(), testContent);
        var httpClient = mockHttp.ToHttpClient();

        using var client = new DownloadClient(httpClient);

        // Act
        var result = await client.DownloadAsync(testUrl, _tempFilePath);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(DownloadState.Completed, result.State);
        Assert.AreEqual(testContent.Length, result.TotalBytes);
        Assert.IsTrue(File.Exists(_tempFilePath));

        var downloadedContent = await File.ReadAllBytesAsync(_tempFilePath);
        CollectionAssert.AreEqual(testContent, downloadedContent);
    }

    /// <summary>
    /// 测试下载 - 带进度报告.
    /// </summary>
    [TestMethod]
    public async Task DownloadAsync_WithProgress_ShouldReportProgress()
    {
        // Arrange
        var testContent = TestDataFactory.CreateRandomBytes(10240); // 10KB
        var testUrl = new Uri("http://test.example.com/file.bin");
        _tempFilePath = TestDataFactory.GetTempFilePath();

        var mockHttp = TestDataFactory.CreateMockHandler(testUrl.ToString(), testContent);
        var httpClient = mockHttp.ToHttpClient();

        using var client = new DownloadClient(httpClient);

        var progressReports = new List<DownloadProgress>();
        var progress = new Progress<DownloadProgress>(p => progressReports.Add(p));

        // Act
        var result = await client.DownloadAsync(testUrl, _tempFilePath, progress: progress);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(progressReports.Count > 0);

        // 最后一个进度应该是完成状态
        var lastProgress = progressReports[^1];
        Assert.AreEqual(DownloadState.Completed, lastProgress.State);
    }

    /// <summary>
    /// 测试下载 - 自定义请求头.
    /// </summary>
    [TestMethod]
    public async Task DownloadAsync_WithHeaders_ShouldIncludeHeaders()
    {
        // Arrange
        var testContent = TestDataFactory.CreateRandomBytes(1024);
        var testUrl = new Uri("http://test.example.com/file.bin");
        _tempFilePath = TestDataFactory.GetTempFilePath();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(testUrl.ToString())
            .WithHeaders("Authorization", "Bearer test-token")
            .Respond("application/octet-stream", new MemoryStream(testContent));

        var httpClient = mockHttp.ToHttpClient();
        using var client = new DownloadClient(httpClient);

        var options = new DownloadOptions
        {
            Headers = { ["Authorization"] = "Bearer test-token" },
        };

        // Act
        var result = await client.DownloadAsync(testUrl, _tempFilePath, options);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    /// 测试下载取消.
    /// </summary>
    [TestMethod]
    public async Task DownloadAsync_Canceled_ShouldReturnCanceledResult()
    {
        // Arrange
        var testContent = TestDataFactory.CreateRandomBytes(1024 * 1024); // 1MB
        var testUrl = new Uri("http://test.example.com/file.bin");
        _tempFilePath = TestDataFactory.GetTempFilePath();

        var mockHttp = TestDataFactory.CreateMockHandler(testUrl.ToString(), testContent);
        var httpClient = mockHttp.ToHttpClient();

        using var client = new DownloadClient(httpClient);
        using var cts = new CancellationTokenSource();

        // 立即取消
        await cts.CancelAsync();

        // Act
        var result = await client.DownloadAsync(testUrl, _tempFilePath, cancellationToken: cts.Token);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(DownloadState.Canceled, result.State);
    }

    /// <summary>
    /// 测试下载 - 文件已存在不覆盖.
    /// </summary>
    [TestMethod]
    public async Task DownloadAsync_FileExists_NoOverwrite_ShouldFail()
    {
        // Arrange
        var testContent = TestDataFactory.CreateRandomBytes(1024);
        var testUrl = new Uri("http://test.example.com/file.bin");
        _tempFilePath = TestDataFactory.GetTempFilePath();

        // 创建已存在的文件
        await File.WriteAllTextAsync(_tempFilePath, "existing content");

        var mockHttp = TestDataFactory.CreateMockHandler(testUrl.ToString(), testContent);
        var httpClient = mockHttp.ToHttpClient();

        using var client = new DownloadClient(httpClient);

        var options = new DownloadOptions { OverwriteExisting = false };

        // Act
        var result = await client.DownloadAsync(testUrl, _tempFilePath, options);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(DownloadState.Failed, result.State);
    }

    /// <summary>
    /// 测试下载 - 文件已存在覆盖.
    /// </summary>
    [TestMethod]
    public async Task DownloadAsync_FileExists_WithOverwrite_ShouldSucceed()
    {
        // Arrange
        var testContent = TestDataFactory.CreateRandomBytes(1024);
        var testUrl = new Uri("http://test.example.com/file.bin");
        _tempFilePath = TestDataFactory.GetTempFilePath();

        // 创建已存在的文件
        await File.WriteAllTextAsync(_tempFilePath, "existing content");

        var mockHttp = TestDataFactory.CreateMockHandler(testUrl.ToString(), testContent);
        var httpClient = mockHttp.ToHttpClient();

        using var client = new DownloadClient(httpClient);

        var options = new DownloadOptions { OverwriteExisting = true };

        // Act
        var result = await client.DownloadAsync(testUrl, _tempFilePath, options);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        var downloadedContent = await File.ReadAllBytesAsync(_tempFilePath);
        CollectionAssert.AreEqual(testContent, downloadedContent);
    }

    /// <summary>
    /// 测试下载 - HTTP 错误.
    /// </summary>
    [TestMethod]
    public async Task DownloadAsync_HttpError_ShouldThrowDownloadException()
    {
        // Arrange
        var testUrl = new Uri("http://test.example.com/file.bin");
        _tempFilePath = TestDataFactory.GetTempFilePath();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(testUrl.ToString())
            .Respond(System.Net.HttpStatusCode.NotFound);

        var httpClient = mockHttp.ToHttpClient();
        using var client = new DownloadClient(httpClient);

        // Act & Assert
        _ = await Assert.ThrowsExactlyAsync<DownloadException>(
            async () => await client.DownloadAsync(testUrl, _tempFilePath));
    }

    /// <summary>
    /// 测试获取文件信息.
    /// </summary>
    [TestMethod]
    public async Task GetFileInfoAsync_ShouldReturnFileInfo()
    {
        // Arrange
        var testContent = TestDataFactory.CreateRandomBytes(1024);
        var testUrl = "http://test.example.com/file.bin";

        var mockHttp = TestDataFactory.CreateMockHandlerWithHead(testUrl, testContent, "application/epub+zip");
        var httpClient = mockHttp.ToHttpClient();

        using var client = new DownloadClient(httpClient);

        // Act
        var info = await client.GetFileInfoAsync(testUrl);

        // Assert
        Assert.AreEqual(testContent.Length, info.ContentLength);
        Assert.AreEqual("application/epub+zip", info.ContentType);
    }

    /// <summary>
    /// 测试 Dispose.
    /// </summary>
    [TestMethod]
    public void Dispose_ShouldDisposeResources()
    {
        // Arrange
        var client = new DownloadClient();

        // Act
        client.Dispose();

        // Assert - 再次调用不应该抛出异常
        client.Dispose();
    }

    /// <summary>
    /// 测试 Dispose 后调用方法.
    /// </summary>
    [TestMethod]
    public async Task DownloadAsync_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var client = new DownloadClient();
        client.Dispose();

        // Act & Assert
        _ = await Assert.ThrowsExactlyAsync<ObjectDisposedException>(
            async () => await client.DownloadAsync(new Uri("http://example.com/file.bin"), "test.bin"));
    }
}

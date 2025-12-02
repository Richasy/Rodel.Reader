// Copyright (c) Richasy. All rights reserved.

namespace DownloadKit.Test.Unit;

/// <summary>
/// DownloadOptions 单元测试.
/// </summary>
[TestClass]
public sealed class DownloadOptionsTests
{
    /// <summary>
    /// 测试默认值.
    /// </summary>
    [TestMethod]
    public void Default_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var options = DownloadOptions.Default;

        // Assert
        Assert.AreEqual(DownloadOptions.DefaultBufferSize, options.BufferSize);
        Assert.AreEqual(DownloadOptions.DefaultProgressThrottleMs, options.ProgressThrottleMs);
        Assert.AreEqual(TimeSpan.FromSeconds(30), options.Timeout);
        Assert.IsFalse(options.OverwriteExisting);
        Assert.IsNull(options.UserAgent);
        Assert.IsNotNull(options.Headers);
        Assert.AreEqual(0, options.Headers.Count);
    }

    /// <summary>
    /// 测试自定义值.
    /// </summary>
    [TestMethod]
    public void Options_ShouldAcceptCustomValues()
    {
        // Arrange & Act
        var options = new DownloadOptions
        {
            BufferSize = 1024 * 1024,
            ProgressThrottleMs = 200,
            Timeout = TimeSpan.FromMinutes(5),
            OverwriteExisting = true,
            UserAgent = "TestAgent/1.0",
            Headers = { ["Authorization"] = "Bearer token" },
        };

        // Assert
        Assert.AreEqual(1024 * 1024, options.BufferSize);
        Assert.AreEqual(200, options.ProgressThrottleMs);
        Assert.AreEqual(TimeSpan.FromMinutes(5), options.Timeout);
        Assert.IsTrue(options.OverwriteExisting);
        Assert.AreEqual("TestAgent/1.0", options.UserAgent);
        Assert.AreEqual("Bearer token", options.Headers["Authorization"]);
    }
}

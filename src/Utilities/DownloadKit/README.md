# DownloadKit

轻量级、AOT 友好的下载工具库，专为 Rodel Reader 设计。

## 特性

- ✅ **AOT 兼容** - 完全支持 Native AOT 编译
- ✅ **线程安全** - 所有操作都是线程安全的
- ✅ **完全异步** - 不会阻塞 UI 线程
- ✅ **进度报告** - 实时进度、速度、剩余时间估算
- ✅ **可测试** - 支持依赖注入，便于单元测试
- ✅ **轻量级** - 无外部依赖，仅使用 .NET 内置功能

## 快速开始

### 基本用法

```csharp
using Richasy.RodelReader.Utilities.DownloadKit;

// 创建客户端
using var client = new DownloadClient();

// 下载文件
var result = await client.DownloadAsync(
    url: "https://example.com/book.epub",
    destinationPath: @"C:\Downloads\book.epub");

if (result.IsSuccess)
{
    Console.WriteLine($"下载完成: {result.FilePath}");
    Console.WriteLine($"文件大小: {result.TotalBytes} 字节");
    Console.WriteLine($"耗时: {result.ElapsedTime}");
}
```

### 带进度报告

```csharp
var progress = new Progress<DownloadProgress>(p =>
{
    Console.WriteLine($"进度: {p.Percentage:F1}%");
    Console.WriteLine($"速度: {p.GetFormattedSpeed()}");
    Console.WriteLine($"已下载: {p.GetFormattedBytesReceived()} / {p.GetFormattedTotalBytes()}");
    
    if (p.EstimatedRemaining.HasValue)
    {
        Console.WriteLine($"剩余时间: {p.EstimatedRemaining}");
    }
});

var result = await client.DownloadAsync(
    url: "https://example.com/large-file.zip",
    destinationPath: @"C:\Downloads\large-file.zip",
    progress: progress);
```

### 自定义配置

```csharp
var options = new DownloadOptions
{
    // 自定义请求头
    Headers =
    {
        ["Authorization"] = "Bearer your-token",
        ["User-Agent"] = "MyApp/1.0"
    },
    
    // 缓冲区大小（默认 80KB）
    BufferSize = 1024 * 1024, // 1MB
    
    // 请求超时（默认 30 秒）
    Timeout = TimeSpan.FromMinutes(5),
    
    // 覆盖已存在的文件
    OverwriteExisting = true,
    
    // 进度报告节流（默认 100ms）
    ProgressThrottleMs = 200
};

var result = await client.DownloadAsync(
    url: "https://example.com/book.epub",
    destinationPath: @"C:\Downloads\book.epub",
    options: options);
```

### 取消下载

```csharp
var cts = new CancellationTokenSource();

// 在其他地方取消
cts.CancelAfter(TimeSpan.FromSeconds(30));
// 或 cts.Cancel();

var result = await client.DownloadAsync(
    url: "https://example.com/large-file.zip",
    destinationPath: @"C:\Downloads\large-file.zip",
    cancellationToken: cts.Token);

if (result.State == DownloadState.Canceled)
{
    Console.WriteLine("下载已取消");
}
```

### 获取远程文件信息

```csharp
var info = await client.GetFileInfoAsync("https://example.com/book.epub");

Console.WriteLine($"文件大小: {info.ContentLength}");
Console.WriteLine($"内容类型: {info.ContentType}");
Console.WriteLine($"最后修改: {info.LastModified}");
Console.WriteLine($"支持断点续传: {info.AcceptRanges}");
```

### 使用自定义 HttpClient

```csharp
// 推荐：使用 IHttpClientFactory
var httpClient = httpClientFactory.CreateClient("downloads");

using var client = new DownloadClient(httpClient);

// 或者手动创建
var handler = new HttpClientHandler
{
    Proxy = new WebProxy("http://proxy.example.com:8080"),
    UseProxy = true
};

var customHttpClient = new HttpClient(handler);
using var client = new DownloadClient(customHttpClient);
```

### 配合日志使用

```csharp
// 使用 Microsoft.Extensions.Logging
ILogger<DownloadClient> logger = loggerFactory.CreateLogger<DownloadClient>();

using var client = new DownloadClient(logger);

// 或者同时提供 HttpClient 和 Logger
using var client = new DownloadClient(httpClient, logger);
```

## API 参考

### DownloadClient

| 方法 | 描述 |
|------|------|
| `DownloadAsync(string url, ...)` | 下载文件（使用 URL 字符串） |
| `DownloadAsync(Uri uri, ...)` | 下载文件（使用 Uri 对象） |
| `GetFileInfoAsync(string url, ...)` | 获取远程文件信息 |

### DownloadOptions

| 属性 | 类型 | 默认值 | 描述 |
|------|------|--------|------|
| `Headers` | `Dictionary<string, string>` | `{}` | 自定义请求头 |
| `BufferSize` | `int` | `81920` | 缓冲区大小（字节） |
| `Timeout` | `TimeSpan` | `30s` | 请求超时时间 |
| `OverwriteExisting` | `bool` | `false` | 是否覆盖已存在文件 |
| `ProgressThrottleMs` | `int` | `100` | 进度报告节流（毫秒） |
| `UserAgent` | `string?` | `null` | 用户代理字符串 |

### DownloadProgress

| 属性 | 类型 | 描述 |
|------|------|------|
| `BytesReceived` | `long` | 已接收字节数 |
| `TotalBytes` | `long?` | 总字节数（可能为 null） |
| `Percentage` | `double?` | 完成百分比 |
| `BytesPerSecond` | `double` | 下载速度 |
| `EstimatedRemaining` | `TimeSpan?` | 预计剩余时间 |
| `State` | `DownloadState` | 当前状态 |

### DownloadResult

| 属性 | 类型 | 描述 |
|------|------|------|
| `IsSuccess` | `bool` | 是否成功 |
| `FilePath` | `string` | 目标文件路径 |
| `TotalBytes` | `long` | 下载的总字节数 |
| `ElapsedTime` | `TimeSpan` | 下载耗时 |
| `State` | `DownloadState` | 最终状态 |
| `Error` | `Exception?` | 错误信息 |
| `AverageSpeed` | `double` | 平均下载速度 |

### DownloadState

| 值 | 描述 |
|------|------|
| `Pending` | 等待中 |
| `Downloading` | 下载中 |
| `Completed` | 已完成 |
| `Canceled` | 已取消 |
| `Failed` | 失败 |

## 异常处理

```csharp
try
{
    var result = await client.DownloadAsync(url, path);
    
    if (!result.IsSuccess)
    {
        // 处理非异常失败（如取消）
        Console.WriteLine($"下载失败: {result.State}");
        Console.WriteLine($"错误: {result.Error?.Message}");
    }
}
catch (DownloadException ex)
{
    Console.WriteLine($"下载异常: {ex.Message}");
    Console.WriteLine($"HTTP 状态码: {ex.StatusCode}");
    Console.WriteLine($"请求 URI: {ex.RequestUri}");
}
catch (DownloadIOException ex)
{
    Console.WriteLine($"IO 异常: {ex.Message}");
    Console.WriteLine($"文件路径: {ex.FilePath}");
}
catch (DownloadCanceledException ex)
{
    Console.WriteLine($"下载已取消，已下载: {ex.BytesDownloaded} 字节");
}
```

## 最佳实践

1. **复用 HttpClient**: 在应用程序中复用 `HttpClient` 实例，避免频繁创建导致的 socket 耗尽问题。

2. **使用 CancellationToken**: 始终传递 `CancellationToken`，以便用户可以取消长时间运行的下载。

3. **进度报告节流**: 使用适当的 `ProgressThrottleMs` 值（默认 100ms）避免 UI 更新过于频繁。

4. **错误处理**: 检查 `DownloadResult.IsSuccess` 和 `DownloadResult.Error`，而不仅仅依赖异常。

5. **日志记录**: 提供 `ILogger` 实例以便调试和监控。

## 许可证

Copyright (c) Richasy. All rights reserved.

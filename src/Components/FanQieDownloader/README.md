# FanQieDownloader

番茄小说下载组件，将 `FanQieSource` 和 `EpubGenerator` 组合，实现完整的番茄小说下载和 EPUB 生成功能。

## 功能特性

- 📥 **增量同步** - 支持连载追更、失败章节重试
- 📊 **双阶段进度** - 下载阶段 + EPUB 生成阶段独立进度通知
- 🔖 **番茄标记** - 保留段落索引和章节 ID，支持运行时获取段评
- 🖼️ **图片嵌入** - 自动下载并嵌入封面和章节内图片
- 💾 **断点续传** - 异常中断时保留缓存，下次可继续
- 📚 **EPUB 即数据源** - 同步时从现有 EPUB 读取已下载信息，无需额外数据库

## 快速开始

```csharp
using Richasy.RodelReader.Components.FanQie;
using Richasy.RodelReader.Components.FanQie.Services;
using Richasy.RodelReader.Sources.FanQie;
using Richasy.RodelPlayer.Utilities.EpubGenerator;

// 创建依赖
using var fanQieClient = new FanQieClient();
var epubBuilder = new EpubBuilder();

// 创建下载服务
var downloadService = new FanQieDownloadService(fanQieClient, epubBuilder);

// 配置选项
var options = new SyncOptions
{
    TempDirectory = @"D:\Temp\FanQie",      // 临时缓存目录
    OutputDirectory = @"D:\Books\Output",    // EPUB 输出目录
    ExistingEpubPath = @"D:\Books\12345.epub", // 现有 EPUB（用于增量同步，可选）
    RetryFailedChapters = true,              // 重试失败章节
    ContinueOnError = true,                  // 下载失败时继续
};

// 同步书籍（带进度回调）
var progress = new Progress<SyncProgress>(p =>
{
    Console.WriteLine($"[{p.Phase}] {p.TotalProgress:F1}% - {p.Message}");
    
    if (p.DownloadDetail != null)
    {
        Console.WriteLine($"  下载: {p.DownloadDetail.Completed}/{p.DownloadDetail.Total}");
    }
});

var result = await downloadService.SyncBookAsync("7046844484302144036", options, progress);

if (result.Success)
{
    Console.WriteLine($"✅ 同步成功: {result.EpubPath}");
    Console.WriteLine($"   新下载: {result.Statistics?.NewlyDownloaded} 章节");
    Console.WriteLine($"   复用: {result.Statistics?.Reused} 章节");
    Console.WriteLine($"   失败: {result.Statistics?.Failed} 章节");
}
else
{
    Console.WriteLine($"❌ 同步失败: {result.ErrorMessage}");
}
```

## 核心概念

### EPUB 即数据源

同步时直接从现有 EPUB 读取已下载章节信息，通过番茄特有标记识别：

```xml
<!-- content.opf 元数据 -->
<meta name="fanqie:book-id" content="7046844484302144036"/>
<meta name="fanqie:sync-time" content="2025-12-03T10:30:00+08:00"/>
<meta name="fanqie:toc-hash" content="ABC123..."/>
<meta name="fanqie:failed-chapters" content="12345,67890"/>
```

### 章节标记

每个段落都包含番茄特有标记，用于运行时获取段评：

```html
<p data-fanqie-index="0" data-fanqie-chapter-id="7046844484302144036">桃源县，雪月楼</p>
<p data-fanqie-index="1" data-fanqie-chapter-id="7046844484302144036">那一年春天...</p>
```

### 失败章节占位

下载失败的章节会生成占位内容，下次同步时自动重试：

```html
<div class="chapter-unavailable" data-fanqie-status="failed">
    <h1>第十章 ???</h1>
    <p class="error-message">由于网络原因，本章节内容暂时无法下载。</p>
    <p class="retry-hint">下次同步时将自动重试。</p>
</div>
```

### 临时缓存

下载过程中使用临时缓存（外部传入路径），正常完成后自动清理：

```
{TempDirectory}/
└── fanqie_{BookId}/
    ├── manifest.json      # 缓存清单（含目录哈希）
    ├── chapters/
    │   ├── {ChapterId}.json
    │   └── ...
    └── images/
        ├── cover
        ├── img_{ChapterId}_{Index}
        └── ...
```

异常中断时保留缓存，下次同步同一本书时可断点续传。

## 同步流程

```
1. 分析现有 EPUB（如果提供）
   ├── 提取已下载章节 ID
   └── 提取失败章节 ID

2. 获取在线书籍信息和目录
   └── 计算目录哈希

3. 检查临时缓存
   ├── 目录哈希一致 → 使用缓存
   └── 目录哈希不一致 → 清空缓存

4. 确定需要下载的章节
   ├── 在线目录 - 已下载 = 新增章节
   └── + 失败章节（如果启用重试）

5. 下载章节
   ├── 调用 FanQieClient.GetChapterContentAsync
   ├── 添加段落标记
   └── 保存到临时缓存

6. 下载图片（封面 + 章节内图片）

7. 生成 EPUB
   ├── 合并：缓存 + 现有 EPUB 复用
   ├── 失败章节 → 占位内容
   └── 添加番茄元数据

8. 清理缓存

9. 返回结果
```

## 进度通知

### SyncPhase 阶段

| 阶段 | 权重 | 说明 |
|------|------|------|
| Analyzing | 0-5% | 分析现有 EPUB |
| FetchingToc | 5-10% | 获取在线目录 |
| CheckingCache | 8-10% | 检查缓存 |
| DownloadingChapters | 10-60% | 下载章节 |
| DownloadingImages | 60-75% | 下载图片 |
| GeneratingEpub | 75-95% | 生成 EPUB |
| CleaningUp | 95-100% | 清理缓存 |

### 进度详情

```csharp
var progress = new Progress<SyncProgress>(p =>
{
    // 总进度
    Console.WriteLine($"总进度: {p.TotalProgress:F1}%");
    
    // 下载阶段详情
    if (p.DownloadDetail != null)
    {
        Console.WriteLine($"下载: {p.DownloadDetail.Completed}/{p.DownloadDetail.Total}");
        Console.WriteLine($"失败: {p.DownloadDetail.Failed}");
        Console.WriteLine($"跳过: {p.DownloadDetail.Skipped}");
        Console.WriteLine($"当前: {p.DownloadDetail.CurrentChapter}");
    }
    
    // 生成阶段详情
    if (p.GenerateDetail != null)
    {
        Console.WriteLine($"生成: {p.GenerateDetail.ProcessedChapters}/{p.GenerateDetail.TotalChapters}");
        Console.WriteLine($"步骤: {p.GenerateDetail.Step}");
    }
});
```

## API 参考

### IFanQieDownloadService

| 方法 | 说明 |
|------|------|
| `SyncBookAsync` | 同步书籍（完整流程） |
| `AnalyzeEpubAsync` | 分析 EPUB 提取番茄信息 |
| `GetCacheStateAsync` | 获取缓存状态 |
| `CleanupCacheAsync` | 清理缓存 |

### SyncOptions

| 属性 | 类型 | 说明 |
|------|------|------|
| `TempDirectory` | `string` | 临时缓存目录（必填） |
| `OutputDirectory` | `string` | EPUB 输出目录（必填） |
| `ExistingEpubPath` | `string?` | 现有 EPUB 路径 |
| `ForceRedownload` | `bool` | 强制重新下载 |
| `RetryFailedChapters` | `bool` | 重试失败章节 |
| `ContinueOnError` | `bool` | 失败时继续 |
| `EpubOptions` | `EpubOptions?` | EPUB 生成选项 |

### SyncResult

| 属性 | 类型 | 说明 |
|------|------|------|
| `Success` | `bool` | 是否成功 |
| `EpubPath` | `string?` | 生成的 EPUB 路径 |
| `BookInfo` | `FanQieBookInfo?` | 书籍信息 |
| `Statistics` | `SyncStatistics?` | 同步统计 |
| `ErrorMessage` | `string?` | 错误信息 |
| `IsCancelled` | `bool` | 是否被取消 |

## 依赖注入

```csharp
services.AddSingleton<IFanQieClient, FanQieClient>();
services.AddSingleton<IEpubBuilder, EpubBuilder>();
services.AddSingleton<IFanQieDownloadService, FanQieDownloadService>();
```

## 许可证

MIT License

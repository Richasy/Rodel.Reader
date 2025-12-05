# LegadoDownloader

Legado（开源阅读）下载组件，将 `LegadoSource` 和 `EpubGenerator` 组合，实现完整的 Legado 书籍下载和 EPUB 生成功能。

## 功能特性

- 📥 **增量同步** - 支持连载追更、失败章节重试
- 📊 **双阶段进度** - 下载阶段 + EPUB 生成阶段独立进度通知
- 🔖 **Legado 标记** - 保留书籍ID、书源ID和服务地址，支持跨设备同步
- 🖼️ **图片嵌入** - 自动下载并嵌入封面和章节内图片（仅完整URL）
- 💾 **断点续传** - 异常中断时保留缓存，下次可继续
- 📚 **EPUB 即数据源** - 同步时从现有 EPUB 读取已下载信息，无需额外数据库

## 快速开始

```csharp
using Richasy.RodelReader.Components.Legado;
using Richasy.RodelReader.Components.Legado.Services;
using Richasy.RodelReader.Sources.Legado;
using Richasy.RodelReader.Utilities.EpubGenerator;

// 创建 Legado 客户端
var clientOptions = new LegadoClientOptions
{
    BaseUrl = "http://192.168.1.100:1234",
    ServerType = ServerType.Legado,
};
using var legadoClient = new LegadoClient(clientOptions);

// 创建 EPUB 构建器
var epubBuilder = new EpubBuilder();

// 创建下载服务
var downloadService = new LegadoDownloadService(legadoClient, epubBuilder);

// 获取书架上的书籍
var books = await legadoClient.GetBookshelfAsync();
var book = books.First();

// 配置同步选项
var options = new SyncOptions
{
    TempDirectory = @"D:\Temp\Legado",       // 临时缓存目录
    OutputDirectory = @"D:\Books\Output",     // EPUB 输出目录
    ExistingEpubPath = @"D:\Books\MyBook.epub", // 现有 EPUB（用于增量同步，可选）
    RetryFailedChapters = true,               // 重试失败章节
    ContinueOnError = true,                   // 下载失败时继续
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

var result = await downloadService.SyncBookAsync(book, options, progress);

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

同步时直接从现有 EPUB 读取已下载章节信息，通过 Legado 特有标记识别：

```xml
<!-- content.opf 元数据 -->
<meta name="legado:book-url" content="https://example.com/book/123"/>
<meta name="legado:book-source" content="https://example.com"/>
<meta name="legado:server-url" content="http://192.168.1.100:1234"/>
<meta name="legado:sync-time" content="2025-12-05T10:30:00+08:00"/>
<meta name="legado:toc-hash" content="ABC123..."/>
<meta name="legado:failed-chapters" content="5,12,45"/>
```

### 章节标记

每个章节都包含索引标记，用于识别章节状态：

```html
<!-- legado:chapter-index=0 -->
<!-- legado:status=downloaded -->
<p>桃源县，雪月楼</p>
<p>那一年春天...</p>
```

### 失败章节占位

下载失败的章节会生成占位内容，下次同步时自动重试：

```html
<!-- legado:chapter-index=10 -->
<!-- legado:status=failed -->
<!-- legado:fail-reason=网络超时 -->
<div class="chapter-unavailable" data-legado-chapter-index="10" data-legado-status="failed">
    <div class="error-content">
        <p class="error-message">由于网络超时，本章节内容暂时无法下载。</p>
        <p class="retry-hint">下次同步时将自动重试。</p>
    </div>
</div>
```

### 图片处理

**重要**：仅下载完整 URL（以 `http://` 或 `https://` 开头）的图片，相对路径图片将被忽略。

```html
<!-- 会被下载并嵌入 -->
<img src="https://example.com/images/cover.jpg"/>

<!-- 会被忽略（保留原样或移除） -->
<img src="/images/cover.jpg"/>
<img src="../images/cover.jpg"/>
```

### 临时缓存

下载过程中使用临时缓存（外部传入路径），正常完成后自动清理：

```
{TempDirectory}/
└── legado_{BookUrlHash}/          # BookUrl 的 MD5 哈希
    ├── manifest.json              # 缓存清单（含目录哈希）
    ├── chapters/
    │   ├── 0.json                 # 使用章节索引作为文件名
    │   ├── 1.json
    │   └── ...
    └── images/
        ├── cover
        ├── img_0_0
        └── ...
```

异常中断时保留缓存，下次同步同一本书时可断点续传。

## 同步流程

```
1. 分析现有 EPUB（如果提供）
   ├── 提取已下载章节索引
   ├── 提取失败章节索引
   └── 验证书源和服务地址

2. 获取书籍章节目录
   └── 计算目录哈希（基于章节 URL 列表）

3. 检查临时缓存
   ├── 目录哈希一致 → 使用缓存
   └── 目录哈希不一致 → 清空缓存

4. 确定需要下载的章节
   ├── 在线目录 - 已下载 = 新增章节
   ├── + 失败章节（如果启用重试）
   └── 分离卷标题和内容章节

5. 下载章节
   ├── 调用 ILegadoClient.GetChapterContentAsync
   ├── 处理 HTML 内容
   ├── 提取完整 URL 图片（忽略相对路径）
   └── 保存到临时缓存

6. 下载图片（封面 + 章节内图片）

7. 生成 EPUB
   ├── 合并：缓存 + 现有 EPUB 复用
   ├── 失败章节 → 占位内容
   └── 添加 Legado 元数据

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

### ILegadoDownloadService

| 方法 | 说明 |
|------|------|
| `SyncBookAsync` | 同步书籍（完整流程） |
| `AnalyzeEpubAsync` | 分析 EPUB 提取 Legado 信息 |
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
| `StartChapterIndex` | `int?` | 起始章节索引（从 0 开始） |
| `EndChapterIndex` | `int?` | 结束章节索引（包含） |

### SyncResult

| 属性 | 类型 | 说明 |
|------|------|------|
| `Success` | `bool` | 是否成功 |
| `EpubPath` | `string?` | 生成的 EPUB 路径 |
| `BookInfo` | `LegadoBookInfo?` | 书籍信息 |
| `Statistics` | `SyncStatistics?` | 同步统计 |
| `ErrorMessage` | `string?` | 错误信息 |
| `IsCancelled` | `bool` | 是否被取消 |

### LegadoBookInfo

| 属性 | 类型 | 说明 |
|------|------|------|
| `BookUrl` | `string` | 书籍链接（唯一标识） |
| `BookSource` | `string` | 书源链接 |
| `ServerUrl` | `string` | 服务地址 |
| `Title` | `string` | 书名 |
| `Author` | `string?` | 作者 |
| `TocHash` | `string?` | 目录哈希 |
| `DownloadedChapterIndexes` | `IReadOnlyList<int>` | 已下载章节索引 |
| `FailedChapterIndexes` | `IReadOnlyList<int>` | 失败章节索引 |

## 与 FanQieDownloader 的差异

| 方面 | FanQieDownloader | LegadoDownloader |
|------|-----------------|------------------|
| **API 地址** | 固定 | 外部传入（通过 `ILegadoClient`） |
| **书籍标识** | `bookId` | `bookUrl`（书籍链接） |
| **EPUB 元数据** | `fanqie:*` | `legado:book-url`、`legado:book-source`、`legado:server-url` 等 |
| **章节标记** | `data-fanqie-index`、`data-fanqie-chapter-id` | 仅索引标记（无段落标记） |
| **章节内容** | 需要解析分段 | 通常是无头 HTML（一堆 `<p>` 标签） |
| **图片处理** | 下载所有图片 | 仅下载完整 URL，忽略相对路径 |
| **卷标题** | 无 | 支持 `IsVolume` 标记 |

## 依赖注入

```csharp
services.AddSingleton<ILegadoClient>(sp =>
{
    var options = new LegadoClientOptions
    {
        BaseUrl = "http://192.168.1.100:1234",
        ServerType = ServerType.Legado,
    };
    return new LegadoClient(options);
});
services.AddSingleton<IEpubBuilder, EpubBuilder>();
services.AddSingleton<ILegadoDownloadService, LegadoDownloadService>();
```

## 许可证

MIT License

# FanQieSource

番茄小说数据源库，用于从番茄小说 API 获取书籍信息和章节内容。

## 功能特性

- 🔍 **搜索书籍** - 根据关键词搜索番茄小说
- 📖 **书籍详情** - 获取书籍的完整元数据
- 📑 **书籍目录** - 获取按卷分组的章节列表
- 📥 **批量下载** - 高效批量获取章节内容（一次最多 25 章）
- 🧹 **内容清洗** - 将 HTML 转换为纯净的文本或 XHTML
- 🖼️ **图片下载** - 支持下载章节中的插图（单张或批量）
- 💬 **段落评论** - 获取章节段落的评论数量和评论列表
- 🔄 **后备 API** - 主 API 不可用时自动切换到后备服务

## 快速开始

```csharp
using Richasy.RodelReader.Sources.FanQie;

// 创建客户端
using var client = new FanQieClient();

// 搜索书籍
var searchResult = await client.SearchBooksAsync("斗破苍穹");
foreach (var book in searchResult.Items)
{
    Console.WriteLine($"{book.Title} - {book.Author}");
}

// 获取书籍详情
var detail = await client.GetBookDetailAsync("1234567890");

// 获取目录
var volumes = await client.GetBookTocAsync("1234567890");

// 批量获取章节内容
var chapters = volumes.SelectMany(v => v.Chapters).Take(10);
var contents = await client.GetChapterContentsAsync(
    detail.BookId,
    detail.Title,
    chapters);

foreach (var content in contents)
{
    Console.WriteLine($"[{content.Order}] {content.Title} - {content.WordCount}字");
}
```

## 图片下载

章节内容中可能包含插图，可以使用图片下载功能获取：

```csharp
// 获取章节内容
var content = await client.GetChapterContentAsync(bookId, bookTitle, chapter);

// 如果有图片，批量下载
if (content?.Images?.Count > 0)
{
    var imageUrls = content.Images.Select(img => img.Url);
    var imageData = await client.DownloadImagesAsync(imageUrls);
    
    foreach (var (url, data) in imageData)
    {
        Console.WriteLine($"下载图片: {url}, 大小: {data.Length} 字节");
    }
}

// 或者下载单张图片
var singleImage = await client.DownloadImageAsync("https://example.com/image.jpg");
```

## 段落评论

获取章节中每个段落的评论数量和评论内容：

```csharp
// 获取段评数量（返回段落索引 -> 评论数量的映射）
var commentCounts = await client.GetCommentCountAsync(bookId, chapterId);
foreach (var (paragraphIndex, count) in commentCounts)
{
    Console.WriteLine($"段落 {paragraphIndex}: {count} 条评论");
}

// 获取特定段落的评论列表
var result = await client.GetCommentsAsync(
    bookId,
    chapterId,
    paragraphIndex: 5);     // 第 5 段

foreach (var comment in result.Comments)
{
    Console.WriteLine($"[{comment.UserName}] {comment.Content}");
    Console.WriteLine($"  👍 {comment.LikeCount}  💬 {comment.ReplyCount}");
}

// 分页获取更多评论
if (result.HasMore)
{
    var moreComments = await client.GetCommentsAsync(
        bookId, chapterId, 5, result.NextOffset);
}
```

## 后备 API

当主 API（`fq.shusan.cn`）不可用时，客户端会自动切换到后备 API。可以通过配置选项控制此行为：

```csharp
var options = new FanQieClientOptions
{
    EnableFallback = true,                              // 启用后备 API（默认开启）
    FallbackApiBaseUrl = "https://fqnovel.richasy.net", // 后备 API 地址
    RequestDelayMs = 100,                               // 请求间隔（毫秒）
};

using var client = new FanQieClient(options);
```

## 与 EpubGenerator 集成

`ChapterContent` 模型可以轻松转换为 `EpubGenerator.ChapterInfo`：

```csharp
var epubChapters = contents.Select(c => new ChapterInfo
{
    Index = c.Order,
    Title = c.Title,
    Content = c.HtmlContent,
    IsHtml = true,
    Images = c.Images?.Select(img => new ChapterImageInfo
    {
        Url = img.Url,
        Offset = img.Offset ?? 0
    }).ToList()
}).ToList();
```

## API 端点

本库使用以下 API 端点获取数据：

| 功能 | 端点 | 来源 |
|------|------|------|
| 搜索 | `api-lf.fanqiesdk.com` | 官方 API |
| 书籍详情 | `api5-normal-sinfonlineb.fqnovel.com` | 官方 API |
| 书籍目录 | `fanqienovel.com/api/reader/directory/detail` | 官方 API |
| 章节内容 | `fq.shusan.cn/api/content` | 第三方 API |
| 后备内容 | `fqnovel.richasy.net/api/fqnovel/*` | 后备 API |
| 段落评论 | `novel.snssdk.com` | 官方 API |

## 第三方依赖

### 主 API

本库使用 [fq.shusan.cn](https://fq.shusan.cn) 提供的第三方 API 来获取章节内容。该服务提供：

- **设备注册** - `/api/device/register` - 获取设备凭证
- **内容获取** - `/api/content` - 获取已解密的章节内容

### 后备 API

当主 API 不可用时，自动切换到 [fqnovel.richasy.net](https://fqnovel.richasy.net) 后备服务：

- **搜索** - `/api/fqnovel/search`
- **书籍详情** - `/api/fqnovel/books/{bookId}`
- **书籍目录** - `/api/fqnovel/chapters/{bookId}/toc`
- **批量章节** - `/api/fqnovel/chapters/batch`

### 注意事项

⚠️ 章节内容获取依赖第三方服务。如果主 API 不可用，客户端会自动尝试后备 API。搜索、详情和目录功能使用官方 API，不受影响。

## 许可证

MIT License


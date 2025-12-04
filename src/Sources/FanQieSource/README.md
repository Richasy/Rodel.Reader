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
- 🔄 **多级 API 回退** - 支持自部署 API 和内置 API 自动切换

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

## 自部署 API

本库支持自部署的第三方 API 服务。可以通过 `SelfHostApiBaseUrl` 配置自己部署的服务地址：

```csharp
var options = new FanQieClientOptions
{
    SelfHostApiBaseUrl = "http://localhost:9999",  // 自部署 API 地址（可选）
    RequestDelayMs = 100,                          // 请求间隔（毫秒）
};

using var client = new FanQieClient(options);
```

### API 请求优先级

1. **官方 API 支持的服务**（搜索、书籍详情、目录）：
   - 官方 API → 自部署 API（如已配置）→ 内置 API

2. **仅第三方支持的服务**（章节内容）：
   - 自部署 API（如已配置）→ 内置 API

### 内置 API

默认使用 `https://fqnovel.richasy.net` 作为内置的第三方 API 服务。

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
| 章节内容 | `fqnovel.richasy.net/api/fqnovel/*` | 第三方 API（内置） |
| 段落评论 | `api5-normal-sinfonlinec.fqnovel.com` | 官方 API |

## 第三方 API 服务

### 内置 API

本库内置使用 [fqnovel.richasy.net](https://fqnovel.richasy.net) 提供的第三方 API 服务：

- **搜索** - `/api/fqsearch/books`
- **书籍详情** - `/api/fqnovel/book/{bookId}`
- **书籍目录** - `/api/fqsearch/directory/{bookId}`
- **批量章节** - `/api/fqnovel/chapters/batch`

### 自部署 API

该第三方服务支持 Docker 自部署，可通过 `SelfHostApiBaseUrl` 配置使用自己部署的服务。

### 注意事项

⚠️ 章节内容获取依赖第三方服务。搜索、详情和目录功能优先使用官方 API，官方 API 失败时会自动回退到第三方 API。

## 许可证

MIT License


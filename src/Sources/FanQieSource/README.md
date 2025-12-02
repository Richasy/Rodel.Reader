# FanQieSource

番茄小说数据源库，用于从番茄小说 API 获取书籍信息和章节内容。

## 功能特性

- 🔍 **搜索书籍** - 根据关键词搜索番茄小说
- 📖 **书籍详情** - 获取书籍的完整元数据
- 📑 **书籍目录** - 获取按卷分组的章节列表
- 📥 **批量下载** - 高效批量获取章节内容（一次最多 25 章）
- 🧹 **内容清洗** - 将 HTML 转换为纯净的文本或 XHTML

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

## 第三方依赖

本库使用 [fq.shusan.cn](https://fq.shusan.cn) 提供的第三方 API 来获取章节内容。该服务提供：

- **设备注册** - `/api/device/register` - 获取设备凭证
- **内容获取** - `/api/content` - 获取已解密的章节内容

### 注意事项

⚠️ 章节内容获取依赖第三方服务，如果该服务不可用，将无法获取章节内容。搜索、详情和目录功能使用官方 API，不受影响。

## 许可证

MIT License

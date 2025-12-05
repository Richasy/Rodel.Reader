# FeedParser

一个高性能的 .NET 9 RSS/Atom 订阅源解析库，支持播客（Podcast）扩展。

## 特性

- 🚀 **高性能** - 使用异步流（`IAsyncEnumerable`）处理大型订阅源
- 📖 **双格式支持** - 同时支持 RSS 2.0 和 Atom 1.0 标准
- 🎙️ **播客支持** - 完整支持 iTunes 播客扩展（duration, image 等）
- 🧩 **接口优先** - 完全依赖注入友好，便于单元测试和扩展
- 🔄 **自动检测** - 自动识别 Feed 类型，无需手动指定
- 📦 **不可变模型** - 使用 record 类型确保线程安全

## 快速开始

### 基本用法

```csharp
using Richasy.RodelReader.Utilities.FeedParser.Readers;

// 从 URL 读取 Feed
using var httpClient = new HttpClient();
await using var stream = await httpClient.GetStreamAsync("https://example.com/feed.xml");

// 自动检测格式并读取
var (channel, items) = await FeedReader.ReadAsync(stream);

Console.WriteLine($"频道: {channel.Title}");
Console.WriteLine($"描述: {channel.Description}");

foreach (var item in items)
{
    Console.WriteLine($"- {item.Title} ({item.PublishedAt})");
}
```

### 使用异步流处理大型 Feed

```csharp
using var reader = await FeedReader.CreateAsync(stream);

var channel = await reader.ReadChannelAsync();
Console.WriteLine($"正在读取: {channel.Title}");

await foreach (var item in reader.ReadItemsAsync())
{
    Console.WriteLine($"- {item.Title}");
    
    // 获取播客附件
    var enclosure = item.GetEnclosure();
    if (enclosure != null)
    {
        Console.WriteLine($"  音频: {enclosure.Uri}");
        Console.WriteLine($"  时长: {item.Duration} 秒");
    }
}
```

### 指定格式读取

```csharp
// RSS
using var rssReader = FeedReader.CreateRssReader(stream);

// Atom
using var atomReader = FeedReader.CreateAtomReader(stream);
```

### 依赖注入

```csharp
// 注册服务
services.AddSingleton<IXmlReaderFactory, XmlReaderFactory>();
services.AddTransient<IFeedParser, RssParser>();
services.AddTransient<IFeedParser, AtomParser>();

// 使用
public class FeedService
{
    private readonly IXmlReaderFactory _xmlReaderFactory;
    
    public FeedService(IXmlReaderFactory xmlReaderFactory)
    {
        _xmlReaderFactory = xmlReaderFactory;
    }
    
    public async Task<FeedChannel> ReadFeedAsync(Stream stream)
    {
        using var reader = await FeedReader.CreateAsync(stream, _xmlReaderFactory);
        return await reader.ReadChannelAsync();
    }
}
```

## 模型

### FeedChannel（频道）

| 属性 | 类型 | 描述 |
|------|------|------|
| Id | string? | 唯一标识符 |
| Title | string | 频道标题 |
| Description | string? | 频道描述 |
| Language | string? | 语言代码 |
| Copyright | string? | 版权信息 |
| Generator | string? | 生成器 |
| LastBuildDate | DateTimeOffset? | 最后更新时间 |
| FeedType | FeedType | Feed 类型 (Rss/Atom) |
| Images | IReadOnlyList\<FeedImage\> | 频道图片 |
| Links | IReadOnlyList\<FeedLink\> | 链接列表 |
| Contributors | IReadOnlyList\<FeedPerson\> | 贡献者 |
| Categories | IReadOnlyList\<FeedCategory\> | 分类 |

### FeedItem（订阅项）

| 属性 | 类型 | 描述 |
|------|------|------|
| Id | string? | 唯一标识符 |
| Title | string | 标题 |
| Description | string? | 摘要描述 |
| Content | string? | 完整内容（HTML） |
| ImageUrl | string? | 封面图片 |
| PublishedAt | DateTimeOffset? | 发布时间 |
| UpdatedAt | DateTimeOffset? | 更新时间 |
| Duration | int? | 音视频时长（秒） |
| Links | IReadOnlyList\<FeedLink\> | 链接列表 |
| Contributors | IReadOnlyList\<FeedPerson\> | 贡献者 |
| Categories | IReadOnlyList\<FeedCategory\> | 分类 |

### FeedLink（链接）

| 属性 | 类型 | 描述 |
|------|------|------|
| Uri | Uri | 链接地址 |
| LinkType | FeedLinkType | 链接类型 |
| Title | string? | 链接标题 |
| MediaType | string? | 媒体类型 |
| Length | long? | 内容长度 |

## 架构

```
FeedParser/
├── Abstractions/           # 接口定义
│   ├── IFeedReader.cs
│   ├── IFeedParser.cs
│   ├── IFeedFormatter.cs
│   ├── IFeedWriter.cs
│   ├── IXmlReaderFactory.cs
│   ├── IXmlWriterFactory.cs
│   └── IFeedElementMapper.cs
├── Models/                 # 数据模型
│   ├── Enums/
│   ├── FeedChannel.cs
│   ├── FeedItem.cs
│   ├── FeedLink.cs
│   └── ...
├── Readers/                # 读取器实现
│   ├── FeedReader.cs       # 门面类
│   ├── RssFeedReader.cs
│   └── AtomFeedReader.cs
├── Parsers/                # 解析器实现
│   ├── RssParser.cs
│   ├── AtomParser.cs
│   ├── RssElementMapper.cs
│   └── AtomElementMapper.cs
├── Helpers/                # 辅助工具
│   ├── DateTimeHelper.cs
│   ├── UriHelper.cs
│   └── ValueConverter.cs
├── Exceptions/             # 异常类型
│   ├── FeedParseException.cs
│   ├── InvalidFeedFormatException.cs
│   └── UnsupportedFeedFormatException.cs
└── Internal/               # 内部实现
    ├── XmlReaderFactory.cs
    ├── RssConstants.cs
    ├── RssElementNames.cs
    ├── AtomConstants.cs
    └── AtomElementNames.cs
```

## 支持的格式

- **RSS 2.0** - 完整支持
- **Atom 1.0** - 完整支持
- **iTunes Podcast** - 支持常用扩展（duration, image, author 等）
- **Content Module** - 支持 content:encoded

## 许可证

MIT License

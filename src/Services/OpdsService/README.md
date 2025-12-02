# OpdsService

一个现代化的 OPDS 1.x 客户端库，用于 .NET 9.0+，支持电子书目录浏览和搜索。

## 特性

- ✅ OPDS 1.2 目录协议支持
- ✅ 模块化设计（目录导航、搜索）
- ✅ 支持身份验证（Basic Auth）
- ✅ 支持依赖注入
- ✅ 可注入的日志组件
- ✅ 异步 API
- ✅ 强类型的数据模型
- ✅ 支持分面导航（Facets）
- ✅ 支持 OpenSearch

## 安装

将项目引用添加到你的解决方案中：

```xml
<ProjectReference Include="..\OpdsService\OpdsService.csproj" />
```

## 快速开始

### 基本使用

```csharp
using Richasy.RodelReader.Services.OpdsService;

// 创建客户端
using var client = OpdsClient.Create(new Uri("https://example.com/opds"));

// 获取根目录
var feed = await client.Catalog.GetRootAsync();

Console.WriteLine($"目录: {feed.Title}");
Console.WriteLine($"条目数: {feed.Entries.Count}");

// 遍历条目
foreach (var entry in feed.Entries)
{
    Console.WriteLine($"- {entry.Title}");
    
    if (entry.IsBookEntry)
    {
        // 这是一本书
        Console.WriteLine($"  作者: {string.Join(", ", entry.Authors.Select(a => a.Name))}");
        
        // 获取下载链接
        var download = entry.GetOpenAccessAcquisition();
        if (download != null)
        {
            Console.WriteLine($"  下载: {download.Href}");
            Console.WriteLine($"  格式: {download.MediaType}");
        }
    }
    else
    {
        // 这是一个子目录
        Console.WriteLine($"  [目录]");
    }
}
```

### 带身份验证

```csharp
using var client = OpdsClient.Create(
    new Uri("https://example.com/opds"),
    "username",
    "password");
```

### 完整配置

```csharp
var options = new OpdsClientOptions
{
    RootUri = new Uri("https://example.com/opds"),
    Credentials = new NetworkCredential("user", "pass"),
    Timeout = TimeSpan.FromSeconds(30),
    UserAgent = "MyReader/1.0",
};

using var client = new OpdsClient(options);
```

### 带日志记录

```csharp
using Microsoft.Extensions.Logging;

var logger = loggerFactory.CreateLogger<OpdsClient>();
using var client = new OpdsClient(options, logger);
```

## 模块化接口

客户端按功能划分为以下模块：

### Catalog（目录导航）

```csharp
// 获取根目录
var root = await client.Catalog.GetRootAsync();

// 获取指定 URI 的目录
var feed = await client.Catalog.GetFeedAsync(new Uri("https://example.com/opds/new"));

// 分页导航
if (feed.HasNextPage)
{
    var nextPage = await client.Catalog.GetNextPageAsync(feed);
}

if (feed.HasPreviousPage)
{
    var prevPage = await client.Catalog.GetPreviousPageAsync(feed);
}

// 导航到子目录
var navEntry = feed.GetNavigationEntries().First();
var subFeed = await client.Catalog.NavigateToEntryAsync(navEntry);
```

### Search（搜索）

```csharp
// 检查是否支持搜索
if (feed.SupportsSearch)
{
    // 方法1：直接搜索
    var results = await client.Search.SearchAsync(feed, "关键词");
    
    // 方法2：获取搜索模板后搜索
    var template = await client.Search.GetSearchTemplateAsync(feed);
    if (template != null)
    {
        var searchUri = client.Search.BuildSearchUri(template, "关键词");
        var results = await client.Search.SearchAsync(template, "关键词");
    }
}
```

## 数据模型

### OpdsFeed（目录）

| 属性 | 类型 | 描述 |
|------|------|------|
| Id | string? | 唯一标识符 |
| Title | string | 目录标题 |
| Subtitle | string? | 副标题 |
| UpdatedAt | DateTimeOffset? | 更新时间 |
| Icon | Uri? | 图标 |
| Entries | IReadOnlyList\<OpdsEntry\> | 条目列表 |
| Links | IReadOnlyList\<OpdsLink\> | 链接列表 |
| FacetGroups | IReadOnlyList\<OpdsFacetGroup\> | 分面组 |

### OpdsEntry（条目）

| 属性 | 类型 | 描述 |
|------|------|------|
| Id | string? | 唯一标识符 |
| Title | string | 标题 |
| Summary | string? | 摘要 |
| Content | string? | 完整内容 |
| UpdatedAt | DateTimeOffset? | 更新时间 |
| PublishedAt | DateTimeOffset? | 发布时间 |
| Language | string? | 语言 |
| Publisher | string? | 出版商 |
| Identifier | string? | 标识符（如 ISBN） |
| Authors | IReadOnlyList\<OpdsAuthor\> | 作者列表 |
| Categories | IReadOnlyList\<OpdsCategory\> | 分类列表 |
| Links | IReadOnlyList\<OpdsLink\> | 链接列表 |
| Images | IReadOnlyList\<OpdsImage\> | 图片列表 |
| Acquisitions | IReadOnlyList\<OpdsAcquisition\> | 获取链接列表 |
| IsNavigationEntry | bool | 是否为导航条目 |
| IsBookEntry | bool | 是否为书籍条目 |

### OpdsAcquisition（获取链接）

| 属性 | 类型 | 描述 |
|------|------|------|
| Type | AcquisitionType | 获取类型（免费、购买等） |
| Href | Uri | 下载/获取地址 |
| MediaType | string? | 媒体类型 |
| Price | OpdsPrice? | 价格信息 |
| IndirectMediaTypes | IReadOnlyList\<string\> | 间接获取的最终格式 |

### AcquisitionType（获取类型）

| 值 | 描述 |
|-----|------|
| Generic | 通用获取 |
| OpenAccess | 开放获取（免费） |
| Borrow | 借阅 |
| Buy | 购买 |
| Sample | 试读/样本 |
| Subscribe | 订阅 |

## 获取下载链接

```csharp
// 获取书籍条目
var bookEntry = feed.GetBookEntries().First();

// 获取免费下载链接
var freeDownload = bookEntry.GetOpenAccessAcquisition();
if (freeDownload != null)
{
    Console.WriteLine($"免费下载: {freeDownload.Href}");
}

// 获取指定格式的下载链接
var epubDownload = bookEntry.GetAcquisitionByMediaType("application/epub+zip");
if (epubDownload != null)
{
    Console.WriteLine($"EPUB 下载: {epubDownload.Href}");
}

// 获取所有可下载链接
foreach (var acquisition in bookEntry.GetDownloadableAcquisitions())
{
    Console.WriteLine($"下载: {acquisition.Href} ({acquisition.MediaType})");
}

// 获取封面图片
var cover = bookEntry.GetCoverImage();
if (cover != null)
{
    Console.WriteLine($"封面: {cover.Href}");
}

// 获取缩略图
var thumbnail = bookEntry.GetThumbnail();
if (thumbnail != null)
{
    Console.WriteLine($"缩略图: {thumbnail.Href}");
}
```

## 分面导航（Facets）

```csharp
// 检查分面组
foreach (var group in feed.FacetGroups)
{
    Console.WriteLine($"分面组: {group.Title}");
    
    foreach (var facet in group.Facets)
    {
        var active = facet.IsActive ? " [激活]" : "";
        var count = facet.Count.HasValue ? $" ({facet.Count})" : "";
        Console.WriteLine($"  - {facet.Title}{count}{active}");
        
        // 点击分面导航
        var facetFeed = await client.Catalog.GetFeedAsync(facet.Href);
    }
}
```

## 架构

```
OpdsService/
├── Abstractions/           # 接口定义
│   ├── IOpdsClient.cs
│   ├── ICatalogNavigator.cs
│   ├── ISearchProvider.cs
│   ├── IOpdsDispatcher.cs
│   └── IOpdsParser.cs
├── Exceptions/             # 异常类型
│   └── OpdsException.cs
├── Helpers/                # 辅助类
│   ├── Guard.cs
│   ├── UriHelper.cs
│   └── DateTimeHelper.cs
├── Internal/               # 内部实现
│   ├── OpdsConstants.cs
│   ├── OpdsElementNames.cs
│   ├── OpdsLinkRelations.cs
│   ├── OpdsDispatcher.cs
│   ├── OpdsV1Parser.cs
│   ├── CatalogNavigator.cs
│   └── SearchProvider.cs
├── Models/                 # 数据模型
│   ├── Enums/
│   │   ├── AcquisitionType.cs
│   │   ├── OpdsLinkRelation.cs
│   │   └── FacetActivation.cs
│   ├── OpdsClientOptions.cs
│   ├── OpdsFeed.cs
│   ├── OpdsEntry.cs
│   ├── OpdsLink.cs
│   ├── OpdsAcquisition.cs
│   ├── OpdsPrice.cs
│   ├── OpdsAuthor.cs
│   ├── OpdsCategory.cs
│   ├── OpdsImage.cs
│   ├── OpdsFacet.cs
│   └── OpdsFacetGroup.cs
├── GlobalUsings.cs
└── OpdsClient.cs           # 主客户端
```

## 未来计划

- [ ] OPDS 2.0 支持（当规范稳定后）
- [ ] 更多认证方式支持

## 许可证

MIT License

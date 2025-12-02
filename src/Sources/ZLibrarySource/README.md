# ZLibrarySource

ZLibrary 客户端库，用于访问 Z-Library 的 API。

## 功能特性

- ✅ 用户登录/登出
- ✅ 书籍搜索（元数据搜索）
- ✅ 全文搜索（书籍内容搜索）
- ✅ 获取书籍详情
- ✅ 获取下载限制信息
- ✅ 获取下载历史
- ✅ 搜索公开/私有书单
- ✅ 获取书单中的书籍
- ✅ AOT 兼容
- ✅ 异步操作

## 安装

项目引用 `ZLibrarySource.csproj` 即可。

## 快速开始

### 基本使用

```csharp
using Richasy.RodelReader.Sources.ZLibrary;

// 创建客户端
using var client = new ZLibraryClient();

// 登录
await client.LoginAsync("your-email@example.com", "your-password");

// 搜索书籍
var searchResult = await client.Search.SearchAsync("Clean Code");

foreach (var book in searchResult.Items)
{
    Console.WriteLine($"{book.Name} by {string.Join(", ", book.Authors ?? [])}");
}

// 获取书籍详情
var detail = await client.Books.GetByIdAsync(searchResult.Items[0].Id);
Console.WriteLine($"Description: {detail.Description}");
Console.WriteLine($"Download URL: {detail.DownloadUrl}");
```

### 高级搜索

```csharp
// 带筛选条件的搜索
var options = new BookSearchOptions
{
    Languages = [BookLanguage.English, BookLanguage.Chinese],
    Extensions = [BookExtension.EPUB, BookExtension.PDF],
    FromYear = 2010,
    ToYear = 2024,
    Exact = false,
    PageSize = 20
};

var result = await client.Search.SearchAsync("programming", page: 1, options);
```

### 全文搜索

```csharp
// 全文搜索（搜索书籍内容）
var fullTextOptions = new FullTextSearchOptions
{
    MatchPhrase = true, // 匹配短语（至少 2 个单词）
    // 或者 MatchWords = true, // 匹配单词
    Languages = [BookLanguage.English],
};

var result = await client.Search.FullTextSearchAsync("design patterns", page: 1, fullTextOptions);
```

### 用户配置

```csharp
// 获取下载限制
var limits = await client.Profile.GetDownloadLimitsAsync();
Console.WriteLine($"今日已下载: {limits.DailyUsed}");
Console.WriteLine($"每日限制: {limits.DailyAllowed}");
Console.WriteLine($"剩余次数: {limits.DailyRemaining}");

// 获取下载历史
var history = await client.Profile.GetDownloadHistoryAsync(
    page: 1,
    fromDate: DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
    toDate: DateOnly.FromDateTime(DateTime.Now)
);
```

### 书单功能

```csharp
// 搜索公开书单
var booklists = await client.Booklists.SearchPublicAsync(
    "science fiction",
    page: 1,
    pageSize: 10,
    order: SortOrder.Popular
);

// 获取书单中的书籍
foreach (var booklist in booklists.Items)
{
    var books = await client.Booklists.GetBooksInListAsync(
        ExtractBooklistId(booklist.Url),
        page: 1
    );
}
```

### 自定义配置

```csharp
var options = new ZLibraryClientOptions
{
    CustomMirror = "https://your-mirror.com", // 自定义镜像
    Timeout = TimeSpan.FromSeconds(120),      // 请求超时
    MaxConcurrentRequests = 32,               // 最大并发数
    UserAgent = "Your Custom User Agent"      // 自定义 UA
};

using var client = new ZLibraryClient(options);
```

### 使用日志

```csharp
using Microsoft.Extensions.Logging;

// 创建日志工厂
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

var logger = loggerFactory.CreateLogger<ZLibraryClient>();

using var client = new ZLibraryClient(new ZLibraryClientOptions(), logger);
```

## 数据模型

### BookItem（书籍基本信息）

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | string | 书籍 ID |
| Name | string | 书籍名称 |
| Isbn | string? | ISBN |
| Url | string? | 详情页 URL |
| CoverUrl | string? | 封面图片 URL |
| Authors | IReadOnlyList<string>? | 作者列表 |
| Publisher | string? | 出版社 |
| Year | string? | 出版年份 |
| Language | string? | 语言 |
| Extension | string? | 文件格式 |
| FileSize | string? | 文件大小 |
| Rating | string? | 评分 |
| Quality | string? | 质量评分 |

### BookDetail（书籍详情）

继承 BookItem 的所有属性，额外包含：

| 属性 | 类型 | 说明 |
|------|------|------|
| Description | string? | 书籍描述 |
| Edition | string? | 版本信息 |
| Isbn10 | string? | ISBN-10 |
| Isbn13 | string? | ISBN-13 |
| Authors | IReadOnlyList<BookAuthor>? | 作者详细信息 |
| Category | BookCategory? | 分类信息 |
| DownloadUrl | string? | 下载链接 |
| IsDownloadAvailable | bool | 是否可下载 |

## 异常处理

```csharp
try
{
    await client.LoginAsync(email, password);
    var result = await client.Search.SearchAsync(query);
}
catch (LoginFailedException ex)
{
    // 登录失败
}
catch (NotAuthenticatedException ex)
{
    // 未登录就调用需要认证的方法
}
catch (EmptyQueryException ex)
{
    // 搜索关键词为空
}
catch (BookNotFoundException ex)
{
    // 书籍未找到
}
catch (ParseException ex)
{
    // HTML 解析失败
}
catch (ZLibraryException ex)
{
    // 其他 ZLibrary 相关异常
}
```

## 注意事项

1. **登录要求**：大部分操作需要先登录，否则会抛出 `NotAuthenticatedException`
2. **下载限制**：Z-Library 有每日下载限制，请使用 `Profile.GetDownloadLimitsAsync()` 检查
3. **网络环境**：某些地区可能需要使用镜像站点
4. **AOT 兼容**：本库完全支持 AOT 编译

## 许可证

Copyright (c) Richasy. All rights reserved.

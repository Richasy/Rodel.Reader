# BookScraper

书籍信息刮削器组件，用于从各种在线服务获取书籍元数据。

## 支持的数据源

| 刮削器 | 服务 | 说明 |
|--------|------|------|
| DouBanBookScraper | 豆瓣读书 | 支持搜索和详情获取 |
| BangumiBookScraper | Bangumi | 支持搜索和详情获取 |
| GoogleBookScraper | Google Books | 使用 Google Books API |
| FanQieBookScraper | 番茄小说 | 复用 FanQieSource |
| JinJiangBookScraper | 晋江文学城 | 支持搜索和详情获取 |
| PixivBookScraper | Pixiv 小说 | 支持搜索 |

## 依赖注入配置

本组件不提供内置的 DI 扩展，需要在应用层自行注册。以下是完整的注册示例：

### 1. 注册基础设施

```csharp
// AngleSharp 浏览上下文工厂（用于 Bangumi、JinJiang）
services.AddSingleton<IBrowsingContextFactory, BrowsingContextFactory>();

// HttpClient 配置
services.AddHttpClient(HttpClientNames.DouBan);  // 豆瓣 API，无需特殊配置

services.AddHttpClient(HttpClientNames.Bangumi, client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
});

services.AddHttpClient(HttpClientNames.Google, client =>
{
    client.BaseAddress = new Uri("https://www.googleapis.com/");
});

services.AddHttpClient(HttpClientNames.JinJiang, client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
});

services.AddHttpClient(HttpClientNames.Pixiv, client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
});
```

### 2. 注册刮削器服务

```csharp
// 按需注册所需的刮削器
services.AddSingleton<DouBanBookScraper>();
services.AddSingleton<BangumiBookScraper>();
services.AddSingleton<GoogleBookScraper>();
services.AddSingleton<JinJiangBookScraper>();
services.AddSingleton<PixivBookScraper>();

// FanQie 刮削器需要先注册 FanQieSource 依赖
services.AddSingleton<IFanQieClient, FanQieClient>();  // 来自 FanQieSource
services.AddSingleton<FanQieBookScraper>();
```

### 3. 注册 IBookScraper 集合（可选）

如果需要通过 `IEnumerable<IBookScraper>` 获取所有刮削器：

```csharp
services.AddSingleton<IBookScraper>(sp => sp.GetRequiredService<DouBanBookScraper>());
services.AddSingleton<IBookScraper>(sp => sp.GetRequiredService<BangumiBookScraper>());
// ... 其他刮削器
```

## HttpClient 名称常量

```csharp
public static class HttpClientNames
{
    public const string DouBan = "DouBan";
    public const string Bangumi = "Bangumi";
    public const string Google = "Google";
    public const string JinJiang = "JinJiang";
    public const string Pixiv = "Pixiv";
}
```

## 直接使用

```csharp
var scraper = new DouBanBookScraper(httpClientFactory, browsingContextFactory, logger);
var results = await scraper.SearchBooksAsync("三体");
```

## 评分说明

所有刮削器返回的评分已标准化为 1-5 分制。

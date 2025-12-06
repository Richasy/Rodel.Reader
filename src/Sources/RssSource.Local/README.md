# RssSource.Local

本地 RSS 源客户端实现。

## 概述

`LocalRssClient` 实现了 `IRssClient` 接口，用于管理本地存储的 RSS 订阅源。它具有以下特点：

- **订阅列表管理**：通过 `IRssStorage` 持久化存储分组和订阅源信息
- **文章获取**：从网络实时获取各订阅源的文章内容
- **OPML 支持**：支持 OPML 导入/导出

## 架构

```
LocalRssClient
├── IRssStorage (依赖注入)
│   └── 管理分组、订阅源的本地存储
├── HttpClient
│   └── 从网络获取 RSS/Atom Feed
└── FeedReader (FeedParser)
    └── 解析 RSS/Atom 格式
```

## 使用方式

```csharp
// 创建存储服务
var storageOptions = new RssStorageOptions { DatabasePath = "rss.db" };
var storage = new RssStorage(storageOptions);
await storage.InitializeAsync();

// 创建客户端
var clientOptions = new LocalRssClientOptions
{
    Timeout = TimeSpan.FromSeconds(30),
    MaxConcurrentRequests = 10,
};

using var client = new LocalRssClient(storage, clientOptions, logger: logger);

// 导入 OPML
var opmlContent = File.ReadAllText("subscriptions.opml");
await client.ImportOpmlAsync(opmlContent);

// 获取订阅源列表
var (groups, feeds) = await client.GetFeedListAsync();

// 获取文章
foreach (var feed in feeds)
{
    var detail = await client.GetFeedDetailAsync(feed);
    Console.WriteLine($"{feed.Name}: {detail?.Articles.Count} 篇文章");
}
```

## 能力

| 能力 | 支持 |
|------|------|
| 需要认证 | ❌ |
| 管理订阅源 | ✅ |
| 管理分组 | ✅ |
| 标记已读 | ✅ |
| 导入 OPML | ✅ |
| 导出 OPML | ✅ |

## 依赖

- `RssSource.Abstractions` - 接口和模型定义
- `RssStorage` - 本地数据存储
- `FeedParser` - RSS/Atom 解析器

# RssSource.Abstractions

RSS 源抽象层，定义了与各种 RSS 在线服务交互的核心接口和模型。

## 概述

此库提供了：

- `IRssClient` - RSS 客户端核心接口
- 公共数据模型（`RssFeed`, `RssFeedGroup`, `RssArticle` 等）
- 配置选项类（`RssClientOptions` 及其派生类）
- 辅助工具类（`Guard`, `HttpClientHelper`）
- 异常类型定义

## 设计原则

1. **AOT 兼容** - 所有代码都兼容 Native AOT 编译
2. **无 ORM 依赖** - 模型类是纯 POCO，不依赖任何 ORM 框架
3. **日志注入** - 通过构造函数注入 `ILogger`
4. **可测试性** - 支持 HttpClient 注入以便于 Mock

## 使用方式

```csharp
// 创建客户端
var options = new ServerBasedRssClientOptions
{
    ServerUrl = "https://your-server.com",
    UserName = "user",
    Password = "pass",
};

using var client = new SomeRssClient(options, logger: logger);

// 登录
await client.SignInAsync();

// 获取订阅源列表
var (groups, feeds) = await client.GetFeedListAsync();

// 获取文章
var detail = await client.GetFeedDetailAsync(feeds[0]);
```

## 项目依赖

- `Microsoft.Extensions.Logging.Abstractions` - 日志抽象

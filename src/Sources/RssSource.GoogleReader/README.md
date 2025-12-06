# RssSource.GoogleReader

Google Reader API 兼容的 RSS 客户端实现。

## 概述

此库实现了 Google Reader API 协议，可用于连接支持该协议的自托管 RSS 服务，如：

- [FreshRSS](https://freshrss.org/)
- [Tiny Tiny RSS](https://tt-rss.org/)
- [Miniflux](https://miniflux.app/) (通过 Google Reader 兼容模式)
- 其他支持 Google Reader API 的服务

## 功能

- ✅ 用户名/密码认证 (ClientLogin)
- ✅ 获取订阅源列表
- ✅ 添加/更新/删除订阅源
- ✅ 获取/重命名/删除分组
- ✅ 获取文章列表
- ✅ 标记文章为已读
- ✅ 导入/导出 OPML

## 使用示例

```csharp
// 创建配置
var options = new GoogleReaderClientOptions
{
    Server = "https://your-freshrss-server.com/api/greader.php",
    UserName = "your-username",
    Password = "your-password",
};

// 创建客户端
using var client = new GoogleReaderClient(options);

// 登录
var loginResult = await client.SignInAsync();
if (!loginResult)
{
    Console.WriteLine("登录失败");
    return;
}

// 获取订阅源列表
var (groups, feeds) = await client.GetFeedListAsync();

// 获取文章
foreach (var feed in feeds)
{
    var detail = await client.GetFeedDetailAsync(feed);
    Console.WriteLine($"{feed.Name}: {detail?.Articles.Count ?? 0} 篇文章");
}
```

## API 兼容性

本库实现了标准的 Google Reader API 端点：

| 端点 | 功能 |
|------|------|
| `/accounts/ClientLogin` | 用户认证 |
| `/reader/api/0/subscription/list` | 获取订阅列表 |
| `/reader/api/0/subscription/edit` | 订阅管理 |
| `/reader/api/0/tag/list` | 获取标签/分组 |
| `/reader/api/0/stream/contents/{id}` | 获取文章流 |
| `/reader/api/0/edit-tag` | 标记已读等操作 |

## AOT 兼容性

本库完全支持 .NET Native AOT 编译，所有 JSON 序列化都使用 Source Generator。

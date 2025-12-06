# RssSource.Miniflux

Miniflux RSS 源客户端库，提供与 Miniflux 服务器的完整交互能力。

## 功能特性

- ✅ 支持 HTTP Basic Auth 和 API Token 两种认证方式
- ✅ 完整的订阅源管理（添加、更新、删除）
- ✅ 完整的分类管理（添加、更新、删除）
- ✅ 文章获取和已读标记
- ✅ OPML 导入/导出
- ✅ AOT 兼容

## 使用示例

```csharp
// 使用用户名密码认证
var options = new MinifluxClientOptions
{
    Server = "https://miniflux.example.com",
    UserName = "your-username",
    Password = "your-password",
};

// 或者使用 API Token
var options = new MinifluxClientOptions
{
    Server = "https://miniflux.example.com",
    ApiToken = "your-api-token",
};

using var client = new MinifluxClient(options);

// 登录
if (await client.SignInAsync())
{
    // 获取订阅源列表
    var (groups, feeds) = await client.GetFeedListAsync();
    
    // 获取订阅源文章
    foreach (var feed in feeds)
    {
        var detail = await client.GetFeedDetailAsync(feed);
        Console.WriteLine($"{feed.Name}: {detail?.Articles.Count} 篇文章");
    }
}
```

## 认证方式

### HTTP Basic Auth

使用用户名和密码进行认证：

```csharp
var options = new MinifluxClientOptions
{
    Server = "https://miniflux.example.com",
    UserName = "username",
    Password = "password",
};
```

### API Token（推荐）

使用 API Token 进行认证（在 Miniflux 设置 -> API Keys 中生成）：

```csharp
var options = new MinifluxClientOptions
{
    Server = "https://miniflux.example.com",
    ApiToken = "your-api-token",
};
```

## API 参考

- [Miniflux API 文档](https://miniflux.app/docs/api.html)

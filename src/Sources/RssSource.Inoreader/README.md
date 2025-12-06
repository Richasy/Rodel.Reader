# Inoreader RSS Source

Inoreader RSS 客户端实现，支持 OAuth 2.0 认证。

## 功能特性

- OAuth 2.0 认证流程
- 订阅源管理（获取/添加/更新/删除）
- 分组管理（获取/更新/删除，不支持直接创建）
- 文章获取和已读状态管理
- OPML 导入/导出
- AOT 编译兼容

## 使用方式

```csharp
// 创建配置选项
var options = new InoreaderClientOptions
{
    AccessToken = "your_access_token",
    RefreshToken = "your_refresh_token",
    ExpireTime = DateTimeOffset.Now.AddHours(1),
    DataSource = InoreaderDataSource.Default,
};

// 创建客户端
var client = new InoreaderClient(options);

// 获取订阅列表
var (groups, feeds) = await client.GetFeedListAsync();

// 获取文章
var detail = await client.GetFeedDetailAsync(feed);
```

## OAuth 认证流程

1. 调用 `InoreaderAuthHelper.GetAuthorizationUrl()` 获取授权 URL
2. 用户在浏览器中完成授权
3. 捕获回调中的授权码
4. 调用 `InoreaderAuthHelper.ExchangeCodeForTokenAsync()` 交换 Token
5. 使用获取的 Token 创建客户端

## 数据源选择

Inoreader 支持多个服务器：
- `Default`: 默认服务器 (www.inoreader.com)
- `Mirror`: 镜像服务器 (www.innoreader.com)
- `Jp`: 日本服务器 (jp.inoreader.com)

## 注意事项

- Inoreader 不支持直接创建分组，分组会在添加订阅源时自动创建
- `AddGroupAsync` 方法会抛出 `NotSupportedException`

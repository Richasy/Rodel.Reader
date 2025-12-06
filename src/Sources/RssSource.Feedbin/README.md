# RssSource.Feedbin

Feedbin RSS 源客户端实现，用于连接 [Feedbin](https://feedbin.com/) 服务。

## 功能特性

- ✅ HTTP Basic 认证
- ✅ 订阅源管理（添加、更新、删除）
- ✅ 文章获取与分页
- ✅ 标签/分组管理（通过 taggings）
- ✅ 标记已读
- ✅ OPML 导入导出
- ✅ AOT 编译兼容

## 使用方法

```csharp
// 创建客户端配置
var options = new FeedbinClientOptions
{
    UserName = "your-username",
    Password = "your-password",
};

// 创建客户端
using var client = new FeedbinClient(options);

// 登录
var success = await client.SignInAsync();

// 获取订阅源列表
var (groups, feeds) = await client.GetFeedListAsync();

// 获取订阅源详情
var detail = await client.GetFeedDetailAsync(feeds[0]);
```

## API 参考

Feedbin API 文档: https://github.com/feedbin/feedbin-api

### 主要端点

| 端点 | 描述 |
|-----|------|
| `GET /v2/authentication.json` | 验证凭据 |
| `GET /v2/subscriptions.json` | 获取订阅列表 |
| `POST /v2/subscriptions.json` | 创建订阅 |
| `GET /v2/feeds/{id}/entries.json` | 获取订阅源文章 |
| `GET /v2/taggings.json` | 获取标签关联 |
| `DELETE /v2/unread_entries.json` | 标记已读 |
| `POST /v2/imports.json` | 导入 OPML |

## 注意事项

1. **分组管理**: Feedbin 不支持直接创建分组，分组通过 taggings 自动创建
2. **ID 区分**: 
   - `subscription_id` 用于删除/更新订阅
   - `feed_id` 用于获取文章
3. **认证方式**: 使用 HTTP Basic Auth，用户名密码 Base64 编码

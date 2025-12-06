# RssSource.NewsBlur

NewsBlur RSS 客户端库，实现 `IRssClient` 接口。

## 功能特性

- 用户名/密码登录认证
- 订阅源管理（添加、删除、重命名）
- 分组管理（添加、删除、重命名）
- 文章获取和已读标记
- OPML 导入/导出
- 支持 AOT 编译

## 使用方法

```csharp
var options = new NewsBlurClientOptions
{
    UserName = "your-username",
    Password = "your-password",
};

using var client = new NewsBlurClient(options);

// 登录
var success = await client.SignInAsync();

// 获取订阅源列表
var (groups, feeds) = await client.GetFeedListAsync();

// 获取文章
var detail = await client.GetFeedDetailAsync(feeds[0]);
```

## API 参考

NewsBlur API 文档: https://newsblur.com/api

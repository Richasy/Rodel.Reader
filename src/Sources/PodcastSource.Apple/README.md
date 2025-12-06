# ApplePodcastSource

一个用于访问 Apple Podcasts 目录的 .NET 9.0+ 客户端库，支持 AOT 编译。

## 特性

- ✅ 获取播客分类
- ✅ 获取分类下的热门播客
- ✅ 搜索播客
- ✅ 获取播客详情（通过 RSS Feed 解析）
- ✅ 模块化设计
- ✅ 支持依赖注入
- ✅ 可注入的日志组件
- ✅ 异步 API
- ✅ AOT 兼容

## 安装

将项目引用添加到你的解决方案中：

```xml
<ProjectReference Include="..\ApplePodcastSource\ApplePodcastSource.csproj" />
```

## 快速开始

### 基本使用

```csharp
using Richasy.RodelReader.Sources.ApplePodcast;

// 创建客户端
using var client = ApplePodcastClient.Create();

// 获取分类列表
var categories = client.Categories.GetCategories();

// 获取某分类下的热门播客
var podcasts = await client.Categories.GetTopPodcastsAsync("1304", "us");

// 搜索播客
var results = await client.Search.SearchAsync("technology");

// 获取播客详情
var detail = await client.Details.GetDetailByIdAsync("123456");
```

### 完整配置

```csharp
var options = new ApplePodcastClientOptions
{
    DefaultRegion = "cn",
    DefaultLimit = 50,
    Timeout = TimeSpan.FromSeconds(30),
    UserAgent = "MyReader/1.0",
};

using var client = new ApplePodcastClient(options);
```

### 带日志记录

```csharp
using Microsoft.Extensions.Logging;

var logger = loggerFactory.CreateLogger<ApplePodcastClient>();
using var client = new ApplePodcastClient(options, logger);
```

## 模块化接口

客户端提供以下模块：

| 模块 | 接口 | 功能 |
|------|------|------|
| Categories | `ICategoryProvider` | 分类管理和热门播客 |
| Search | `IPodcastSearcher` | 搜索播客 |
| Details | `IPodcastDetailProvider` | 获取播客详情 |

## API 端点

本库使用以下 Apple iTunes API：

- 热门播客: `https://itunes.apple.com/{region}/rss/toppodcasts/limit={limit}/genre={genreId}/json`
- 播客查询: `https://itunes.apple.com/lookup?id={id}&entity=podcast`
- 播客搜索: `https://itunes.apple.com/search?term={keyword}&media=podcast&entity=podcast&limit={limit}`

## 许可证

MIT License

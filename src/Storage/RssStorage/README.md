# RssStorage

RSS 本地存储服务，提供订阅源、文章、阅读状态和收藏的持久化存储功能。

## 概述

`RssStorage` 是 Rodel.Reader 项目中用于 RSS 数据本地缓存的存储层。它与 `RssSource` 系列项目配合使用，实现**读写分离**架构：

- **RssSource**：负责从各种 RSS 在线服务（如 Feedly、Inoreader 等）获取内容
- **RssStorage**：负责本地缓存和持久化存储

## 特性

- 🚀 **高性能** - 基于 SQLite 的轻量级本地数据库
- 🔧 **AOT 兼容** - 完全兼容 Native AOT 编译
- 📦 **无 ORM 依赖** - 使用原生 SQL，无额外依赖
- 🔒 **线程安全** - 支持异步操作，正确处理资源释放
- 📝 **日志支持** - 集成 `Microsoft.Extensions.Logging`

## 安装

```xml
<PackageReference Include="Richasy.RodelReader.Storage.Rss" />
```

## 快速开始

### 基本使用

```csharp
using Richasy.RodelReader.Storage.Rss;
using Richasy.RodelReader.Sources.Rss.Abstractions;

// 1. 创建存储选项
var options = new RssStorageOptions
{
    DatabasePath = "path/to/rss.db",
    CreateTablesOnInit = true
};

// 2. 创建并初始化存储实例
await using var storage = new RssStorage(options);
await storage.InitializeAsync();

// 3. 现在可以使用存储服务了
var feeds = await storage.GetAllFeedsAsync();
```

### 与 RssSource 配合使用

```csharp
// 从 RSS 服务获取数据
using var rssClient = new SomeRssClient(clientOptions);
await rssClient.SignInAsync();

var (groups, feeds) = await rssClient.GetFeedListAsync();

// 缓存到本地存储
await storage.UpsertGroupsAsync(groups);
await storage.UpsertFeedsAsync(feeds);

// 获取订阅源详情并缓存文章
foreach (var feed in feeds)
{
    var detail = await rssClient.GetFeedDetailAsync(feed);
    if (detail?.Articles != null)
    {
        await storage.UpsertArticlesAsync(detail.Articles);
    }
}
```

## API 参考

### 初始化

```csharp
// 初始化存储（创建数据库和表）
await storage.InitializeAsync(cancellationToken);
```

### 订阅源 (Feed) 操作

```csharp
// 获取所有订阅源
IReadOnlyList<RssFeed> feeds = await storage.GetAllFeedsAsync();

// 根据 ID 获取订阅源
RssFeed? feed = await storage.GetFeedAsync(feedId);

// 添加或更新订阅源
await storage.UpsertFeedAsync(feed);

// 批量添加或更新订阅源
await storage.UpsertFeedsAsync(feeds);

// 删除订阅源
bool deleted = await storage.DeleteFeedAsync(feedId);
```

### 分组 (Group) 操作

```csharp
// 获取所有分组
IReadOnlyList<RssFeedGroup> groups = await storage.GetAllGroupsAsync();

// 根据 ID 获取分组
RssFeedGroup? group = await storage.GetGroupAsync(groupId);

// 添加或更新分组
await storage.UpsertGroupAsync(group);

// 批量添加或更新分组
await storage.UpsertGroupsAsync(groups);

// 删除分组
bool deleted = await storage.DeleteGroupAsync(groupId);
```

### 文章 (Article) 操作

```csharp
// 获取订阅源下的文章（不含内容，用于列表展示）
IReadOnlyList<RssArticle> articles = await storage.GetArticlesByFeedAsync(
    feedId,
    limit: 50,
    offset: 0);

// 获取未读文章
IReadOnlyList<RssArticle> unread = await storage.GetUnreadArticlesAsync(
    feedId: null,  // null 表示所有订阅源
    limit: 50,
    offset: 0);

// 获取收藏文章
IReadOnlyList<RssArticle> favorites = await storage.GetFavoriteArticlesAsync(
    limit: 50,
    offset: 0);

// 获取文章详情（含完整内容）
RssArticle? article = await storage.GetArticleAsync(articleId);

// 仅获取文章内容
string? content = await storage.GetArticleContentAsync(articleId);

// 添加或更新文章
await storage.UpsertArticleAsync(article);

// 批量添加或更新文章
await storage.UpsertArticlesAsync(articles);

// 删除文章
bool deleted = await storage.DeleteArticleAsync(articleId);

// 删除订阅源下的所有文章
int deletedCount = await storage.DeleteArticlesByFeedAsync(feedId);
```

### 阅读状态管理

```csharp
// 标记文章为已读
await storage.MarkAsReadAsync(new[] { articleId1, articleId2 });

// 标记文章为未读
await storage.MarkAsUnreadAsync(new[] { articleId1, articleId2 });

// 将订阅源下所有文章标记为已读
await storage.MarkFeedAsReadAsync(feedId);

// 将所有文章标记为已读
await storage.MarkAllAsReadAsync();

// 检查文章是否已读
bool isRead = await storage.IsArticleReadAsync(articleId);
```

### 收藏管理

```csharp
// 添加收藏
await storage.AddFavoriteAsync(articleId);

// 移除收藏
await storage.RemoveFavoriteAsync(articleId);

// 检查是否已收藏
bool isFavorite = await storage.IsArticleFavoriteAsync(articleId);
```

### 数据清理

```csharp
// 清理过期文章（保留收藏）
int cleanedCount = await storage.CleanupOldArticlesAsync(
    olderThan: DateTimeOffset.Now.AddDays(-30),
    keepFavorites: true);

// 清空所有数据
await storage.ClearAllAsync();
```

## 配置选项

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `DatabasePath` | `string` | `""` | 数据库文件路径 |
| `CreateTablesOnInit` | `bool` | `true` | 是否在初始化时自动创建表 |

## 数据库 Schema

存储使用 SQLite 数据库，包含以下表：

### Groups（分组表）

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | TEXT PRIMARY KEY | 分组标识符 |
| `Name` | TEXT NOT NULL | 分组名称 |

### Feeds（订阅源表）

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | TEXT PRIMARY KEY | 订阅源标识符 |
| `Name` | TEXT NOT NULL | 订阅源名称 |
| `Url` | TEXT NOT NULL | 订阅源 URL |
| `Website` | TEXT | 网站地址 |
| `Description` | TEXT | 描述 |
| `IconUrl` | TEXT | 图标 URL |
| `GroupIds` | TEXT | 所属分组 ID（逗号分隔） |
| `Comment` | TEXT | 备注 |
| `IsFullContentRequired` | INTEGER | 是否需要完整内容 |

### Articles（文章表）

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | TEXT PRIMARY KEY | 文章标识符 |
| `FeedId` | TEXT NOT NULL | 所属订阅源 ID（外键） |
| `Title` | TEXT NOT NULL | 标题 |
| `Summary` | TEXT | 摘要 |
| `Content` | TEXT | 完整内容（HTML） |
| `CoverUrl` | TEXT | 封面图片 URL |
| `Url` | TEXT | 文章链接 |
| `Author` | TEXT | 作者 |
| `PublishTime` | TEXT | 发布时间（ISO 8601） |
| `Tags` | TEXT | 标签（逗号分隔） |
| `ExtraData` | TEXT | 额外数据（JSON） |
| `CachedAt` | TEXT NOT NULL | 缓存时间 |

### ReadStatus（阅读状态表）

| 字段 | 类型 | 说明 |
|------|------|------|
| `ArticleId` | TEXT PRIMARY KEY | 文章 ID（外键） |
| `ReadAt` | TEXT NOT NULL | 阅读时间 |

### Favorites（收藏表）

| 字段 | 类型 | 说明 |
|------|------|------|
| `ArticleId` | TEXT PRIMARY KEY | 文章 ID（外键） |
| `FavoritedAt` | TEXT NOT NULL | 收藏时间 |

## 架构说明

### 读写分离设计

```
┌─────────────────┐     获取数据     ┌─────────────────┐
│   RssSource     │ ───────────────► │  在线 RSS 服务   │
│   (IRssClient)  │ ◄─────────────── │  (Feedly 等)    │
└────────┬────────┘                  └─────────────────┘
         │
         │ 缓存数据
         ▼
┌─────────────────┐
│   RssStorage    │
│  (IRssStorage)  │
└────────┬────────┘
         │
         │ 持久化
         ▼
┌─────────────────┐
│   SQLite DB     │
└─────────────────┘
```

### 仓库模式

存储层内部采用仓库模式，每种数据类型都有独立的 Repository：

- `FeedRepository` - 订阅源数据操作
- `GroupRepository` - 分组数据操作
- `ArticleRepository` - 文章数据操作
- `ReadStatusRepository` - 阅读状态操作
- `FavoriteRepository` - 收藏操作

## 依赖

- `Microsoft.Data.Sqlite` - SQLite 数据库访问
- `Microsoft.Extensions.Logging.Abstractions` - 日志抽象
- `RssSource.Abstractions` - RSS 模型定义

## 注意事项

1. **初始化顺序**：必须先调用 `InitializeAsync()` 才能使用其他方法
2. **资源释放**：使用 `await using` 或 `using` 确保正确释放资源
3. **外键约束**：删除订阅源时会级联删除相关文章
4. **文章内容**：列表查询默认不包含 `Content` 字段以优化性能

## 许可证

Copyright (c) Richasy. All rights reserved.

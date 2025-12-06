# PodcastStorage

播客存储服务库，提供播客订阅、单集、收听进度和收听历史的本地存储功能。

## 功能特性

- **播客管理**：订阅、分组、来源类型标注（常规播客/哔哩哔哩）
- **单集管理**：缓存、元数据存储
- **收听进度**：记录每个单集的播放位置和进度
- **收听历史**：细化的收听时段记录，支持统计分析
- **分组支持**：播客可归类到多个分组

## 使用方式

```csharp
var options = new PodcastStorageOptions
{
    DatabasePath = "path/to/podcast.db",
    CreateTablesOnInit = true
};

await using var storage = new PodcastStorage(options, logger);
await storage.InitializeAsync();

// 添加播客
var podcast = new Podcast
{
    Id = Guid.NewGuid().ToString(),
    Title = "我的播客",
    FeedUrl = "https://example.com/feed.xml",
    SourceType = PodcastSourceType.Standard,
    AddedAt = DateTimeOffset.UtcNow.ToString("O"),
    UpdatedAt = DateTimeOffset.UtcNow.ToString("O"),
};
await storage.UpsertPodcastAsync(podcast);

// 添加收听时段
var session = new ListeningSession
{
    Id = Guid.NewGuid().ToString(),
    EpisodeId = episodeId,
    PodcastId = podcastId,
    StartedAt = startTime.ToString("O"),
    EndedAt = endTime.ToString("O"),
    DurationSeconds = 1800, // 30分钟
};
await storage.AddSessionAsync(session);

// 获取统计
var stats = await storage.GetPodcastStatsAsync(podcastId);
Console.WriteLine($"总收听时长: {stats.TotalListeningTime}");
```

## 数据模型

- `Podcast` - 播客订阅
- `PodcastGroup` - 播客分组
- `Episode` - 单集
- `ListeningProgress` - 收听进度
- `ListeningSession` - 收听时段
- `ListeningStats` - 收听统计

## 技术实现

- 使用 SQLite 作为存储引擎
- 利用 `SqliteGenerator` 源生成器减少模板代码
- 完善的日志记录
- AOT 兼容

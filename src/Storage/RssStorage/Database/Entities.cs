// Copyright (c) Richasy. All rights reserved.

using Richasy.SqliteGenerator;

namespace Richasy.RodelReader.Storage.Rss.Database;

/// <summary>
/// RSS 文章实体（数据库映射）.
/// </summary>
[SqliteTable("Articles")]
internal sealed partial class RssArticleEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string FeedId { get; set; } = string.Empty;

    [SqliteColumn]
    public string Title { get; set; } = string.Empty;

    [SqliteColumn]
    public string? Summary { get; set; }

    [SqliteColumn(ExcludeFromList = true)]
    public string? Content { get; set; }

    [SqliteColumn]
    public string? CoverUrl { get; set; }

    [SqliteColumn]
    public string? Url { get; set; }

    [SqliteColumn]
    public string? Author { get; set; }

    [SqliteColumn]
    public string? PublishTime { get; set; }

    [SqliteColumn]
    public string? Tags { get; set; }

    [SqliteColumn]
    public string? ExtraData { get; set; }

    [SqliteColumn(IsAutoTimestamp = true)]
    public long CachedAt { get; set; }

    /// <summary>
    /// 从 RssArticle 转换.
    /// </summary>
    public static RssArticleEntity FromModel(Sources.Rss.Abstractions.RssArticle article)
    {
        return new RssArticleEntity
        {
            Id = article.Id,
            FeedId = article.FeedId,
            Title = article.Title,
            Summary = article.Summary,
            Content = article.Content,
            CoverUrl = article.CoverUrl,
            Url = article.Url,
            Author = article.Author,
            PublishTime = article.PublishTime,
            Tags = article.Tags,
            ExtraData = article.ExtraData,
        };
    }

    /// <summary>
    /// 转换为 RssArticle.
    /// </summary>
    public Sources.Rss.Abstractions.RssArticle ToModel()
    {
        return new Sources.Rss.Abstractions.RssArticle
        {
            Id = Id,
            FeedId = FeedId,
            Title = Title,
            Summary = Summary,
            Content = Content,
            CoverUrl = CoverUrl,
            Url = Url,
            Author = Author,
            PublishTime = PublishTime,
            Tags = Tags,
            ExtraData = ExtraData,
        };
    }
}

/// <summary>
/// RSS 订阅源实体（数据库映射）.
/// </summary>
[SqliteTable("Feeds")]
internal sealed partial class RssFeedEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string Name { get; set; } = string.Empty;

    [SqliteColumn]
    public string Url { get; set; } = string.Empty;

    [SqliteColumn]
    public string? Website { get; set; }

    [SqliteColumn]
    public string? Description { get; set; }

    [SqliteColumn]
    public string? IconUrl { get; set; }

    [SqliteColumn]
    public string? GroupIds { get; set; }

    [SqliteColumn]
    public string? Comment { get; set; }

    [SqliteColumn]
    public bool IsFullContentRequired { get; set; }

    /// <summary>
    /// 从 RssFeed 转换.
    /// </summary>
    public static RssFeedEntity FromModel(Sources.Rss.Abstractions.RssFeed feed)
    {
        return new RssFeedEntity
        {
            Id = feed.Id,
            Name = feed.Name,
            Url = feed.Url,
            Website = feed.Website,
            Description = feed.Description,
            IconUrl = feed.IconUrl,
            GroupIds = feed.GroupIds,
            Comment = feed.Comment,
            IsFullContentRequired = feed.IsFullContentRequired,
        };
    }

    /// <summary>
    /// 转换为 RssFeed.
    /// </summary>
    public Sources.Rss.Abstractions.RssFeed ToModel()
    {
        return new Sources.Rss.Abstractions.RssFeed
        {
            Id = Id,
            Name = Name,
            Url = Url,
            Website = Website,
            Description = Description,
            IconUrl = IconUrl,
            GroupIds = GroupIds,
            Comment = Comment,
            IsFullContentRequired = IsFullContentRequired,
        };
    }
}

/// <summary>
/// RSS 分组实体（数据库映射）.
/// </summary>
[SqliteTable("Groups")]
internal sealed partial class RssFeedGroupEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 从 RssFeedGroup 转换.
    /// </summary>
    public static RssFeedGroupEntity FromModel(Sources.Rss.Abstractions.RssFeedGroup group)
    {
        return new RssFeedGroupEntity
        {
            Id = group.Id,
            Name = group.Name,
        };
    }

    /// <summary>
    /// 转换为 RssFeedGroup.
    /// </summary>
    public Sources.Rss.Abstractions.RssFeedGroup ToModel()
    {
        return new Sources.Rss.Abstractions.RssFeedGroup
        {
            Id = Id,
            Name = Name,
        };
    }
}

/// <summary>
/// 阅读状态实体（数据库映射）.
/// </summary>
[SqliteTable("ReadStatus")]
internal sealed partial class ReadStatusEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string ArticleId { get; set; } = string.Empty;

    [SqliteColumn(IsAutoTimestamp = true)]
    public long ReadAt { get; set; }
}

/// <summary>
/// 收藏实体（数据库映射）.
/// </summary>
[SqliteTable("Favorites")]
internal sealed partial class FavoriteEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string ArticleId { get; set; } = string.Empty;

    [SqliteColumn(IsAutoTimestamp = true)]
    public long FavoritedAt { get; set; }
}

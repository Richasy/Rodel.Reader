// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 文章.
/// </summary>
public sealed class RssArticle
{
    /// <summary>
    /// 标识符.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 所属订阅源 ID.
    /// </summary>
    public string FeedId { get; set; } = string.Empty;

    /// <summary>
    /// 标题.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 摘要.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 完整内容（HTML）.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 封面图片地址.
    /// </summary>
    public string? CoverUrl { get; set; }

    /// <summary>
    /// 文章链接.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 作者.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 发布时间（ISO 8601 格式）.
    /// </summary>
    public string? PublishTime { get; set; }

    /// <summary>
    /// 标签（逗号分隔）.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 额外参数（JSON 格式，用于服务特定数据）.
    /// </summary>
    public string? ExtraData { get; set; }

    /// <summary>
    /// 获取标签列表.
    /// </summary>
    /// <returns>标签列表.</returns>
    public IReadOnlyList<string> GetTagList()
        => string.IsNullOrEmpty(Tags)
            ? []
            : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// 设置标签列表.
    /// </summary>
    /// <param name="tags">标签列表.</param>
    public void SetTagList(IEnumerable<string> tags)
        => Tags = string.Join(',', tags);

    /// <summary>
    /// 获取发布时间.
    /// </summary>
    /// <returns>发布时间，解析失败返回 null.</returns>
    public DateTimeOffset? GetPublishTime()
        => DateTimeOffset.TryParse(PublishTime, out var time) ? time : null;

    /// <summary>
    /// 设置发布时间.
    /// </summary>
    /// <param name="time">发布时间.</param>
    public void SetPublishTime(DateTimeOffset time)
        => PublishTime = time.ToString("O");

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is RssArticle article && Id == article.Id;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(Id);
}

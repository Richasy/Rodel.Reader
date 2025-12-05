// Copyright (c) Reader Copilot. All rights reserved.

using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Richasy.ReaderKernel.Models.Rss;

/// <summary>
/// RSS 文章.
/// </summary>
public class RssArticleBase
{
    /// <summary>
    /// 标识符.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    [Key]
    public string Id { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 总结.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 内容.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 封面图片地址.
    /// </summary>
    public string? Cover { get; set; }

    /// <summary>
    /// 对应的网址.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 作者.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 发布时间.
    /// </summary>
    public string PublishDate { get; set; }

    /// <summary>
    /// 标签.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 额外参数.
    /// </summary>
    public string? ExtraParameter { get; set; }

    /// <summary>
    /// 所属订阅源ID.
    /// </summary>
    public string FeedId { get; set; }

    /// <summary>
    /// 是否为收藏文章.
    /// </summary>
    [JsonConverter(typeof(BoolJsonConverter))]
    public bool IsFavorites { get; set; }

    /// <summary>
    /// 是否为稍后阅读文章.
    /// </summary>
    [JsonConverter(typeof(BoolJsonConverter))]
    public bool IsReadLater { get; set; }

    /// <summary>
    /// 转换.
    /// </summary>
    public RssCacheArticle ToCache()
    {
        return new RssCacheArticle
        {
            Id = Id,
            Title = Title,
            Summary = Summary,
            Content = Content,
            Cover = Cover,
            Url = Url,
            Author = Author,
            PublishDate = PublishDate,
            Tags = Tags,
            ExtraParameter = ExtraParameter,
            FeedId = FeedId,
            IsFavorites = IsFavorites,
            IsReadLater = IsReadLater,
        };
    }

    /// <summary>
    /// 转换.
    /// </summary>
    /// <returns></returns>
    public RssStoreArticle ToStore()
    {
        return new RssStoreArticle
        {
            Id = Id,
            Title = Title,
            Summary = Summary,
            Content = Content,
            Cover = Cover,
            Url = Url,
            Author = Author,
            PublishDate = PublishDate,
            Tags = Tags,
            ExtraParameter = ExtraParameter,
            FeedId = FeedId,
            IsFavorites = IsFavorites,
            IsReadLater = IsReadLater,
        };
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is RssArticleBase article && Id == article.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary>
    /// 获取标签列表.
    /// </summary>
    /// <returns>ID列表.</returns>
    public List<string> GetTags()
        => string.IsNullOrEmpty(Tags) ? [] : [.. Tags.Split(',')];

    /// <summary>
    /// 设置标签列表.
    /// </summary>
    /// <param name="tags">标签列表.</param>
    public void SetTags(IEnumerable<string> tags)
        => Tags = string.Join(',', tags);

    /// <summary>
    /// 添加标签.
    /// </summary>
    /// <param name="tag">ID.</param>
    public void AddTag(string tag)
    {
        var list = GetTags();
        if (!list.Contains(tag))
        {
            list.Add(tag);
            SetTags(list);
        }
    }
}

/// <summary>
/// RSS 文章.
/// </summary>
[SugarTable("Articles")]
[Table("Articles")]
public sealed class RssCacheArticle : RssArticleBase;

/// <summary>
/// RSS 文章.
/// </summary>
[SugarTable("RssArticle")]
[Table("RssArticle")]
public sealed class RssStoreArticle : RssArticleBase;
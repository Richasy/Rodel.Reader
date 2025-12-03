// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Models;

/// <summary>
/// 段评信息.
/// </summary>
public sealed class Comment
{
    /// <summary>
    /// 评论 ID.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 评论内容.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// 用户 ID.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// 用户名.
    /// </summary>
    public string? UserName { get; init; }

    /// <summary>
    /// 用户头像 URL.
    /// </summary>
    public Uri? Avatar { get; init; }

    /// <summary>
    /// 是否是作者.
    /// </summary>
    public bool IsAuthor { get; init; }

    /// <summary>
    /// 发布时间.
    /// </summary>
    public DateTime PublishTime { get; init; }

    /// <summary>
    /// 点赞数.
    /// </summary>
    public int LikeCount { get; init; }

    /// <summary>
    /// 回复数.
    /// </summary>
    public int ReplyCount { get; init; }

    /// <summary>
    /// 评论中的图片列表.
    /// </summary>
    public IReadOnlyList<Uri>? Pictures { get; init; }
}

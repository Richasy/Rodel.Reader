// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.FanQie.Models.Internal;

/// <summary>
/// 通用数据响应包装.
/// </summary>
/// <typeparam name="T">数据类型.</typeparam>
internal sealed class DataResponse<T>
{
    /// <summary>
    /// 响应码.
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 响应消息.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 数据.
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

/// <summary>
/// 段评数量数据.
/// </summary>
internal sealed class CommentCountData
{
    /// <summary>
    /// 段落索引与评论数量的映射.
    /// </summary>
    [JsonPropertyName("idea_data")]
    public Dictionary<string, CommentCountItem>? IdeaData { get; set; }
}

/// <summary>
/// 段评数量项.
/// </summary>
internal sealed class CommentCountItem
{
    /// <summary>
    /// 评论数量.
    /// </summary>
    [JsonPropertyName("idea_count")]
    public int IdeaCount { get; set; }
}

/// <summary>
/// 段评列表数据.
/// </summary>
internal sealed class CommentListData
{
    /// <summary>
    /// 评论列表.
    /// </summary>
    [JsonPropertyName("comments")]
    public IList<CommentItem>? Comments { get; set; }

    /// <summary>
    /// 评论总数.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>
    /// 是否还有更多.
    /// </summary>
    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    /// <summary>
    /// 下一页偏移量.
    /// </summary>
    [JsonPropertyName("next_offset")]
    public long NextOffset { get; set; }

    /// <summary>
    /// 下一页类型.
    /// </summary>
    [JsonPropertyName("next_page_type")]
    public int NextPageType { get; set; }

    /// <summary>
    /// 过滤数量.
    /// </summary>
    [JsonPropertyName("filter_count")]
    public int FilterCount { get; set; }

    /// <summary>
    /// 段落原始内容.
    /// </summary>
    [JsonPropertyName("para_src_content")]
    public string? ParaSrcContent { get; set; }
}

/// <summary>
/// 评论项.
/// </summary>
internal sealed class CommentItem
{
    /// <summary>
    /// 评论 ID.
    /// </summary>
    [JsonPropertyName("comment_id")]
    public string? CommentId { get; set; }

    /// <summary>
    /// 分组 ID.
    /// </summary>
    [JsonPropertyName("group_id")]
    public string? GroupId { get; set; }

    /// <summary>
    /// 用户信息.
    /// </summary>
    [JsonPropertyName("user_info")]
    public CommentUserInfo? UserInfo { get; set; }

    /// <summary>
    /// 评论内容.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 创建时间戳.
    /// </summary>
    [JsonPropertyName("create_timestamp")]
    public long CreateTimestamp { get; set; }

    /// <summary>
    /// 评论类型.
    /// </summary>
    [JsonPropertyName("comment_type")]
    public int CommentType { get; set; }

    /// <summary>
    /// 回复数量.
    /// </summary>
    [JsonPropertyName("reply_count")]
    public int ReplyCount { get; set; }

    /// <summary>
    /// 点赞数量.
    /// </summary>
    [JsonPropertyName("digg_count")]
    public int DiggCount { get; set; }

    /// <summary>
    /// 是否可以用户删除.
    /// </summary>
    [JsonPropertyName("can_user_del")]
    public bool CanUserDel { get; set; }

    /// <summary>
    /// 是否有作者点赞.
    /// </summary>
    [JsonPropertyName("has_author_digg")]
    public bool HasAuthorDigg { get; set; }

    /// <summary>
    /// 服务 ID.
    /// </summary>
    [JsonPropertyName("service_id")]
    public int ServiceId { get; set; }

    /// <summary>
    /// 书籍 ID.
    /// </summary>
    [JsonPropertyName("book_id")]
    public string? BookId { get; set; }

    /// <summary>
    /// 创建者 ID.
    /// </summary>
    [JsonPropertyName("creator_id")]
    public string? CreatorId { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// 等级.
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; set; }

    /// <summary>
    /// 小说段落哈希码.
    /// </summary>
    [JsonPropertyName("novel_para_hash_code")]
    public string? NovelParaHashCode { get; set; }

    /// <summary>
    /// 图片 URL 列表.
    /// </summary>
    [JsonPropertyName("image_url")]
    public IList<string>? ImageUrl { get; set; }

    /// <summary>
    /// 是否可以分享.
    /// </summary>
    [JsonPropertyName("can_share")]
    public bool CanShare { get; set; }

    /// <summary>
    /// 扩展图片 URL 列表.
    /// </summary>
    [JsonPropertyName("expand_image_url")]
    public IList<string>? ExpandImageUrl { get; set; }

    /// <summary>
    /// 阅读时长.
    /// </summary>
    [JsonPropertyName("read_duration")]
    public int ReadDuration { get; set; }

    /// <summary>
    /// 应用 ID.
    /// </summary>
    [JsonPropertyName("app_id")]
    public int AppId { get; set; }

    /// <summary>
    /// 反对数量.
    /// </summary>
    [JsonPropertyName("disagree_count")]
    public int DisagreeCount { get; set; }

    /// <summary>
    /// 用户是否反对.
    /// </summary>
    [JsonPropertyName("user_disagree")]
    public bool UserDisagree { get; set; }

    /// <summary>
    /// 是否有回复.
    /// </summary>
    [JsonPropertyName("has_reply")]
    public bool HasReply { get; set; }

    /// <summary>
    /// 精选状态.
    /// </summary>
    [JsonPropertyName("select_status")]
    public int SelectStatus { get; set; }

    /// <summary>
    /// 转发数量.
    /// </summary>
    [JsonPropertyName("forwarded_count")]
    public int ForwardedCount { get; set; }
}

/// <summary>
/// 评论用户信息.
/// </summary>
internal sealed class CommentUserInfo
{
    /// <summary>
    /// 用户 ID.
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// 用户类型.
    /// </summary>
    [JsonPropertyName("user_type")]
    public int UserType { get; set; }

    /// <summary>
    /// 用户名.
    /// </summary>
    [JsonPropertyName("user_name")]
    public string? UserName { get; set; }

    /// <summary>
    /// 用户头像.
    /// </summary>
    [JsonPropertyName("user_avatar")]
    public string? UserAvatar { get; set; }

    /// <summary>
    /// 是否是作者.
    /// </summary>
    [JsonPropertyName("is_author")]
    public bool IsAuthor { get; set; }

    /// <summary>
    /// 性别.
    /// </summary>
    [JsonPropertyName("gender")]
    public int Gender { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

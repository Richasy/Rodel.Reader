// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.NewsBlur.Internal;

/// <summary>
/// NewsBlur 故事列表响应.
/// </summary>
internal sealed class NewsBlurStoriesResponse
{
    /// <summary>
    /// 故事列表.
    /// </summary>
    [JsonPropertyName("stories")]
    public List<NewsBlurStory>? Stories { get; set; }

    /// <summary>
    /// 是否认证成功.
    /// </summary>
    [JsonPropertyName("authenticated")]
    public bool Authenticated { get; set; }
}

/// <summary>
/// NewsBlur 添加订阅源响应.
/// </summary>
internal sealed class NewsBlurAddFeedResponse
{
    /// <summary>
    /// 结果代码.
    /// 1=成功, -1=已存在, 其他=失败.
    /// </summary>
    [JsonPropertyName("result")]
    public int Result { get; set; }

    /// <summary>
    /// 新添加的订阅源.
    /// </summary>
    [JsonPropertyName("feed")]
    public NewsBlurFeed? Feed { get; set; }

    /// <summary>
    /// 错误消息.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// NewsBlur 通用操作响应.
/// </summary>
internal sealed class NewsBlurOperationResponse
{
    /// <summary>
    /// 结果代码.
    /// </summary>
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    /// <summary>
    /// 是否认证成功.
    /// </summary>
    [JsonPropertyName("authenticated")]
    public bool Authenticated { get; set; }

    /// <summary>
    /// 错误消息.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

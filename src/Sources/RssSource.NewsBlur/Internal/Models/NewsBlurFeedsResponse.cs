// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.NewsBlur.Internal;

/// <summary>
/// NewsBlur 订阅源列表响应.
/// </summary>
internal sealed class NewsBlurFeedsResponse
{
    /// <summary>
    /// 订阅源字典（键为订阅源 ID）.
    /// </summary>
    [JsonPropertyName("feeds")]
    public Dictionary<string, NewsBlurFeed>? Feeds { get; set; }

    /// <summary>
    /// 文件夹列表.
    /// </summary>
    [JsonPropertyName("folders")]
    public JsonElement? Folders { get; set; }

    /// <summary>
    /// 是否认证成功.
    /// </summary>
    [JsonPropertyName("authenticated")]
    public bool Authenticated { get; set; }
}

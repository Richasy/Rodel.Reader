// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.NewsBlur.Internal;

/// <summary>
/// NewsBlur 登录响应.
/// </summary>
internal sealed class NewsBlurAuthResult
{
    /// <summary>
    /// 是否认证成功.
    /// </summary>
    [JsonPropertyName("authenticated")]
    public bool Authenticated { get; set; }

    /// <summary>
    /// 用户 ID.
    /// </summary>
    [JsonPropertyName("user_id")]
    public int? UserId { get; set; }

    /// <summary>
    /// 错误信息.
    /// </summary>
    [JsonPropertyName("errors")]
    public Dictionary<string, List<string>>? Errors { get; set; }
}

// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.GoogleReader.Internal;

/// <summary>
/// 登录响应.
/// </summary>
internal sealed class GoogleReaderAuthResponse
{
    /// <summary>
    /// Session ID.
    /// </summary>
    [JsonPropertyName("SID")]
    public string? SID { get; set; }

    /// <summary>
    /// Long Session ID.
    /// </summary>
    [JsonPropertyName("LSID")]
    public string? LSID { get; set; }

    /// <summary>
    /// 认证令牌.
    /// </summary>
    [JsonPropertyName("Auth")]
    public string? Auth { get; set; }
}

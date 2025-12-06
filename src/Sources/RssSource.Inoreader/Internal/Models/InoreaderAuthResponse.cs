// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Inoreader.Internal;

/// <summary>
/// OAuth 认证响应.
/// </summary>
internal sealed class InoreaderAuthResponse
{
    /// <summary>
    /// 访问令牌.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 刷新令牌.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间（秒）.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; set; }

    /// <summary>
    /// 令牌类型.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// 作用域.
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}

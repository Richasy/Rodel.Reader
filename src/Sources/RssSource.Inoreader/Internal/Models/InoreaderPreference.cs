// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Inoreader.Internal;

/// <summary>
/// 流偏好设置响应.
/// </summary>
internal sealed class InoreaderPreferenceResponse
{
    /// <summary>
    /// 流偏好设置.
    /// </summary>
    [JsonPropertyName("streamprefs")]
    public Dictionary<string, List<InoreaderPreference>> StreamPrefs { get; set; } = [];
}

/// <summary>
/// 偏好设置项.
/// </summary>
internal sealed class InoreaderPreference
{
    /// <summary>
    /// 设置 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 设置值.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

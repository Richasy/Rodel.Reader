// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Models;

/// <summary>
/// 章节信息.
/// </summary>
public sealed class Chapter
{
    /// <summary>
    /// 章节链接.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 章节标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 是否为卷标题.
    /// </summary>
    [JsonPropertyName("isVolume")]
    public bool IsVolume { get; set; }

    /// <summary>
    /// 基础链接.
    /// </summary>
    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; set; }

    /// <summary>
    /// 书籍链接.
    /// </summary>
    [JsonPropertyName("bookUrl")]
    public string? BookUrl { get; set; }

    /// <summary>
    /// 章节索引.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// 标签.
    /// </summary>
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 资源链接.
    /// </summary>
    [JsonPropertyName("resourceUrl")]
    public string? ResourceUrl { get; set; }

    /// <summary>
    /// 是否付费.
    /// </summary>
    [JsonPropertyName("isVip")]
    public bool IsVip { get; set; }

    /// <summary>
    /// 是否已购买.
    /// </summary>
    [JsonPropertyName("isPay")]
    public bool IsPay { get; set; }

    /// <summary>
    /// 开始位置.
    /// </summary>
    [JsonPropertyName("start")]
    public long? Start { get; set; }

    /// <summary>
    /// 结束位置.
    /// </summary>
    [JsonPropertyName("end")]
    public long? End { get; set; }
}

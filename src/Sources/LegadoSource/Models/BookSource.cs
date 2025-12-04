// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Models;

/// <summary>
/// 书源信息.
/// </summary>
public sealed class BookSource
{
    /// <summary>
    /// 书源链接（唯一标识）.
    /// </summary>
    [JsonPropertyName("bookSourceUrl")]
    public string BookSourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// 书源名称.
    /// </summary>
    [JsonPropertyName("bookSourceName")]
    public string? BookSourceName { get; set; }

    /// <summary>
    /// 书源分组.
    /// </summary>
    [JsonPropertyName("bookSourceGroup")]
    public string? BookSourceGroup { get; set; }

    /// <summary>
    /// 书源类型 (0: 小说, 1: 漫画, 2: 音频).
    /// </summary>
    [JsonPropertyName("bookSourceType")]
    public int BookSourceType { get; set; }

    /// <summary>
    /// 书源注释.
    /// </summary>
    [JsonPropertyName("bookSourceComment")]
    public string? BookSourceComment { get; set; }

    /// <summary>
    /// 是否启用.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 是否启用发现.
    /// </summary>
    [JsonPropertyName("enabledExplore")]
    public bool EnabledExplore { get; set; } = true;

    /// <summary>
    /// 权重.
    /// </summary>
    [JsonPropertyName("weight")]
    public int Weight { get; set; }

    /// <summary>
    /// 自定义排序.
    /// </summary>
    [JsonPropertyName("customOrder")]
    public int CustomOrder { get; set; }

    /// <summary>
    /// 最后更新时间.
    /// </summary>
    [JsonPropertyName("lastUpdateTime")]
    public long LastUpdateTime { get; set; }

    /// <summary>
    /// 响应时间.
    /// </summary>
    [JsonPropertyName("respondTime")]
    public long RespondTime { get; set; }

    /// <summary>
    /// 登录链接.
    /// </summary>
    [JsonPropertyName("loginUrl")]
    public string? LoginUrl { get; set; }

    /// <summary>
    /// 登录 UI.
    /// </summary>
    [JsonPropertyName("loginUi")]
    public string? LoginUi { get; set; }

    /// <summary>
    /// 登录检查 JS.
    /// </summary>
    [JsonPropertyName("loginCheckJs")]
    public string? LoginCheckJs { get; set; }

    /// <summary>
    /// 书源头信息.
    /// </summary>
    [JsonPropertyName("header")]
    public string? Header { get; set; }

    /// <summary>
    /// 搜索链接.
    /// </summary>
    [JsonPropertyName("searchUrl")]
    public string? SearchUrl { get; set; }

    /// <summary>
    /// 发现链接.
    /// </summary>
    [JsonPropertyName("exploreUrl")]
    public string? ExploreUrl { get; set; }

    /// <summary>
    /// 原始 JSON 数据（用于保留未解析的字段）.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

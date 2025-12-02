// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ApplePodcast.Models;

/// <summary>
/// Apple Podcast 客户端配置选项.
/// </summary>
public sealed class ApplePodcastClientOptions
{
    /// <summary>
    /// 默认区域代码 (如 "us", "cn", "jp").
    /// </summary>
    public string DefaultRegion { get; set; } = "us";

    /// <summary>
    /// 默认返回数量限制.
    /// </summary>
    public int DefaultLimit { get; set; } = 100;

    /// <summary>
    /// HTTP 请求超时时间.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 自定义 User-Agent.
    /// </summary>
    public string? UserAgent { get; set; }
}

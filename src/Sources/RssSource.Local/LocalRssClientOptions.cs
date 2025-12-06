// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Local;

/// <summary>
/// 本地 RSS 客户端配置选项.
/// </summary>
public sealed class LocalRssClientOptions : RssClientOptions
{
    /// <summary>
    /// 获取或设置用户代理字符串.
    /// </summary>
    public string? UserAgent { get; set; }
}

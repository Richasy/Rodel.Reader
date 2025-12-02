// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// 下载历史条目.
/// </summary>
public sealed record DownloadHistoryItem
{
    /// <summary>
    /// 获取书籍名称.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 获取书籍详情 URL.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// 获取下载日期.
    /// </summary>
    public string? Date { get; init; }
}

// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// 订阅源类型.
/// </summary>
public enum FeedType
{
    /// <summary>
    /// 未知类型.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// RSS 2.0 格式.
    /// </summary>
    Rss,

    /// <summary>
    /// Atom 1.0 格式.
    /// </summary>
    Atom,
}

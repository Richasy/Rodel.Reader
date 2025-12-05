// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Abstractions;

/// <summary>
/// 插件能力标识.
/// 使用位标志以支持一个插件提供多种能力.
/// </summary>
[Flags]
public enum PluginCapability
{
    /// <summary>
    /// 无能力.
    /// </summary>
    None = 0,

    /// <summary>
    /// 书籍刮削能力 - 可从在线服务获取书籍元数据.
    /// </summary>
    BookScraper = 1 << 0,

    /// <summary>
    /// 书籍源能力 - 可提供书籍内容下载（预留）.
    /// </summary>
    BookSource = 1 << 1,

    /// <summary>
    /// 书籍解析能力 - 可解析特定格式的电子书文件（预留）.
    /// </summary>
    BookParser = 1 << 2,

    /// <summary>
    /// 书籍导出能力 - 可将书籍导出为特定格式（预留）.
    /// </summary>
    BookExporter = 1 << 3,
}

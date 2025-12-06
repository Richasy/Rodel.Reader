// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 源能力元数据接口.
/// 定义 RSS 源支持的功能特性.
/// </summary>
public interface IRssSourceCapabilities
{
    /// <summary>
    /// 获取源标识符.
    /// </summary>
    string SourceId { get; }

    /// <summary>
    /// 获取源显示名称.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// 获取是否需要身份验证.
    /// </summary>
    bool RequiresAuthentication { get; }

    /// <summary>
    /// 获取认证类型.
    /// </summary>
    RssAuthType AuthType { get; }

    /// <summary>
    /// 获取是否支持添加/删除/更新订阅源.
    /// </summary>
    bool CanManageFeeds { get; }

    /// <summary>
    /// 获取是否支持添加/删除/更新分组.
    /// </summary>
    bool CanManageGroups { get; }

    /// <summary>
    /// 获取是否支持标记文章为已读.
    /// </summary>
    bool CanMarkAsRead { get; }

    /// <summary>
    /// 获取是否支持导入 OPML.
    /// </summary>
    bool CanImportOpml { get; }

    /// <summary>
    /// 获取是否支持导出 OPML（所有源都应支持本地导出）.
    /// </summary>
    bool CanExportOpml { get; }
}

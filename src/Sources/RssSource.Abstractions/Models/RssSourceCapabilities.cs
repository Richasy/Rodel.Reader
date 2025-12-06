// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 源能力元数据实现.
/// </summary>
public sealed record RssSourceCapabilities : IRssSourceCapabilities
{
    /// <inheritdoc/>
    public required string SourceId { get; init; }

    /// <inheritdoc/>
    public required string DisplayName { get; init; }

    /// <inheritdoc/>
    public bool RequiresAuthentication { get; init; }

    /// <inheritdoc/>
    public RssAuthType AuthType { get; init; }

    /// <inheritdoc/>
    public bool CanManageFeeds { get; init; } = true;

    /// <inheritdoc/>
    public bool CanManageGroups { get; init; } = true;

    /// <inheritdoc/>
    public bool CanMarkAsRead { get; init; } = true;

    /// <inheritdoc/>
    public bool CanImportOpml { get; init; }

    /// <inheritdoc/>
    public bool CanExportOpml { get; init; } = true;
}

/// <summary>
/// 已知 RSS 源的能力定义.
/// </summary>
public static class KnownRssSources
{
    /// <summary>
    /// 本地订阅源.
    /// </summary>
    public static readonly RssSourceCapabilities Local = new()
    {
        SourceId = "local",
        DisplayName = "本地订阅",
        RequiresAuthentication = false,
        AuthType = RssAuthType.None,
        CanManageFeeds = true,
        CanManageGroups = true,
        CanMarkAsRead = true,
        CanImportOpml = true,
        CanExportOpml = true,
    };

    /// <summary>
    /// Inoreader 服务.
    /// </summary>
    public static readonly RssSourceCapabilities Inoreader = new()
    {
        SourceId = "inoreader",
        DisplayName = "Inoreader",
        RequiresAuthentication = true,
        AuthType = RssAuthType.OAuth,
        CanManageFeeds = true,
        CanManageGroups = true,
        CanMarkAsRead = true,
        CanImportOpml = true,
        CanExportOpml = true,
    };

    /// <summary>
    /// Miniflux 服务.
    /// </summary>
    public static readonly RssSourceCapabilities Miniflux = new()
    {
        SourceId = "miniflux",
        DisplayName = "Miniflux",
        RequiresAuthentication = true,
        AuthType = RssAuthType.Basic,
        CanManageFeeds = true,
        CanManageGroups = true,
        CanMarkAsRead = true,
        CanImportOpml = true,
        CanExportOpml = true,
    };

    /// <summary>
    /// Feedbin 服务.
    /// </summary>
    public static readonly RssSourceCapabilities Feedbin = new()
    {
        SourceId = "feedbin",
        DisplayName = "Feedbin",
        RequiresAuthentication = true,
        AuthType = RssAuthType.Basic,
        CanManageFeeds = true,
        CanManageGroups = false, // Feedbin 不支持分组管理
        CanMarkAsRead = true,
        CanImportOpml = true,
        CanExportOpml = true,
    };

    /// <summary>
    /// Google Reader API 兼容服务.
    /// </summary>
    public static readonly RssSourceCapabilities GoogleReader = new()
    {
        SourceId = "google-reader",
        DisplayName = "Google Reader API",
        RequiresAuthentication = true,
        AuthType = RssAuthType.Basic,
        CanManageFeeds = true,
        CanManageGroups = true,
        CanMarkAsRead = true,
        CanImportOpml = false,
        CanExportOpml = true,
    };

    /// <summary>
    /// NewsBlur 服务.
    /// </summary>
    public static readonly RssSourceCapabilities NewsBlur = new()
    {
        SourceId = "newsblur",
        DisplayName = "NewsBlur",
        RequiresAuthentication = true,
        AuthType = RssAuthType.Basic,
        CanManageFeeds = true,
        CanManageGroups = true,
        CanMarkAsRead = true,
        CanImportOpml = true,
        CanExportOpml = true,
    };
}

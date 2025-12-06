// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Miniflux.Internal;

/// <summary>
/// Miniflux 订阅源.
/// </summary>
internal sealed class MinifluxFeed
{
    /// <summary>
    /// Feed ID.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// 用户 ID.
    /// </summary>
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    /// <summary>
    /// Feed 标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Feed URL.
    /// </summary>
    [JsonPropertyName("feed_url")]
    public string? FeedUrl { get; set; }

    /// <summary>
    /// 网站 URL.
    /// </summary>
    [JsonPropertyName("site_url")]
    public string? SiteUrl { get; set; }

    /// <summary>
    /// 最后检查时间.
    /// </summary>
    [JsonPropertyName("checked_at")]
    public DateTimeOffset CheckedAt { get; set; }

    /// <summary>
    /// 下次检查时间.
    /// </summary>
    [JsonPropertyName("next_check_at")]
    public DateTimeOffset? NextCheckAt { get; set; }

    /// <summary>
    /// ETag 头.
    /// </summary>
    [JsonPropertyName("etag_header")]
    public string? EtagHeader { get; set; }

    /// <summary>
    /// Last-Modified 头.
    /// </summary>
    [JsonPropertyName("last_modified_header")]
    public string? LastModifiedHeader { get; set; }

    /// <summary>
    /// 解析错误消息.
    /// </summary>
    [JsonPropertyName("parsing_error_message")]
    public string? ParsingErrorMessage { get; set; }

    /// <summary>
    /// 解析错误次数.
    /// </summary>
    [JsonPropertyName("parsing_error_count")]
    public int ParsingErrorCount { get; set; }

    /// <summary>
    /// 抓取规则.
    /// </summary>
    [JsonPropertyName("scraper_rules")]
    public string? ScraperRules { get; set; }

    /// <summary>
    /// 重写规则.
    /// </summary>
    [JsonPropertyName("rewrite_rules")]
    public string? RewriteRules { get; set; }

    /// <summary>
    /// 是否启用爬虫.
    /// </summary>
    [JsonPropertyName("crawler")]
    public bool Crawler { get; set; }

    /// <summary>
    /// 黑名单规则.
    /// </summary>
    [JsonPropertyName("blocklist_rules")]
    public string? BlocklistRules { get; set; }

    /// <summary>
    /// 白名单规则.
    /// </summary>
    [JsonPropertyName("keeplist_rules")]
    public string? KeeplistRules { get; set; }

    /// <summary>
    /// URL 重写规则.
    /// </summary>
    [JsonPropertyName("urlrewrite_rules")]
    public string? UrlrewriteRules { get; set; }

    /// <summary>
    /// 自定义 User-Agent.
    /// </summary>
    [JsonPropertyName("user_agent")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Cookie.
    /// </summary>
    [JsonPropertyName("cookie")]
    public string? Cookie { get; set; }

    /// <summary>
    /// Feed 用户名.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Feed 密码.
    /// </summary>
    [JsonPropertyName("password")]
    public string? Password { get; set; }

    /// <summary>
    /// 是否禁用.
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否禁用媒体播放器.
    /// </summary>
    [JsonPropertyName("no_media_player")]
    public bool NoMediaPlayer { get; set; }

    /// <summary>
    /// 是否忽略 HTTP 缓存.
    /// </summary>
    [JsonPropertyName("ignore_http_cache")]
    public bool IgnoreHttpCache { get; set; }

    /// <summary>
    /// 是否允许自签名证书.
    /// </summary>
    [JsonPropertyName("allow_self_signed_certificates")]
    public bool AllowSelfSignedCertificates { get; set; }

    /// <summary>
    /// 是否通过代理获取.
    /// </summary>
    [JsonPropertyName("fetch_via_proxy")]
    public bool FetchViaProxy { get; set; }

    /// <summary>
    /// 所属分类.
    /// </summary>
    [JsonPropertyName("category")]
    public MinifluxCategory? Category { get; set; }

    /// <summary>
    /// 图标信息.
    /// </summary>
    [JsonPropertyName("icon")]
    public MinifluxIcon? Icon { get; set; }

    /// <summary>
    /// 是否全局隐藏.
    /// </summary>
    [JsonPropertyName("hide_globally")]
    public bool HideGlobally { get; set; }

    /// <summary>
    /// Apprise 服务 URL.
    /// </summary>
    [JsonPropertyName("apprise_service_urls")]
    public string? AppriseServiceUrls { get; set; }
}

/// <summary>
/// Miniflux 图标信息.
/// </summary>
internal sealed class MinifluxIcon
{
    /// <summary>
    /// Feed ID.
    /// </summary>
    [JsonPropertyName("feed_id")]
    public long FeedId { get; set; }

    /// <summary>
    /// 图标 ID.
    /// </summary>
    [JsonPropertyName("icon_id")]
    public long IconId { get; set; }
}

/// <summary>
/// Miniflux 图标数据.
/// </summary>
internal sealed class MinifluxIconData
{
    /// <summary>
    /// 图标 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// 图标数据（Base64）.
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    /// <summary>
    /// MIME 类型.
    /// </summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }
}

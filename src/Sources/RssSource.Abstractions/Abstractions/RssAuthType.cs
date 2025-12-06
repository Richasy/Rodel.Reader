// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 源认证类型.
/// </summary>
public enum RssAuthType
{
    /// <summary>
    /// 无需认证（本地源）.
    /// </summary>
    None,

    /// <summary>
    /// 基本认证（用户名+密码）.
    /// </summary>
    Basic,

    /// <summary>
    /// API Token 认证.
    /// </summary>
    Token,

    /// <summary>
    /// OAuth 认证流程.
    /// </summary>
    OAuth,
}

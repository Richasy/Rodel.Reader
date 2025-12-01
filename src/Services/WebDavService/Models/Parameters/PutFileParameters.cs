// Copyright (c) Richasy. All rights reserved.

using System.Net.Http.Headers;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// PUT 文件操作参数.
/// </summary>
public sealed class PutFileParameters : RequestParameters
{
    /// <summary>
    /// 获取或设置锁令牌.
    /// </summary>
    public string? LockToken { get; set; }

    /// <summary>
    /// 获取或设置文件的 MIME 类型.
    /// </summary>
    public new MediaTypeHeaderValue? ContentType { get; set; }
}

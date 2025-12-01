// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// SEARCH 操作参数.
/// </summary>
public sealed class SearchParameters : RequestParameters
{
    /// <summary>
    /// 获取或设置搜索范围 URI.
    /// </summary>
    public string? SearchScope { get; set; }

    /// <summary>
    /// 获取或设置搜索关键字.
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 获取或设置自定义搜索请求体.
    /// </summary>
    /// <remarks>
    /// 如果设置了此属性，将忽略 SearchScope 和 Keyword 属性.
    /// </remarks>
    public string? RawSearchRequest { get; set; }

    /// <summary>
    /// 验证参数有效性.
    /// </summary>
    /// <exception cref="InvalidOperationException">参数无效时抛出.</exception>
    public void Validate()
    {
        if (string.IsNullOrEmpty(RawSearchRequest) && string.IsNullOrEmpty(Keyword))
        {
            throw new InvalidOperationException("必须提供 Keyword 或 RawSearchRequest.");
        }
    }
}

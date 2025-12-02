// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Helpers;

/// <summary>
/// URI 辅助类.
/// </summary>
internal static class UriHelper
{
    /// <summary>
    /// 尝试解析 URI 字符串.
    /// </summary>
    /// <param name="uriString">URI 字符串.</param>
    /// <param name="result">解析结果.</param>
    /// <returns>是否解析成功.</returns>
    public static bool TryParse(string? uriString, out Uri? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(uriString))
        {
            return false;
        }

        return Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out result);
    }

    /// <summary>
    /// 解析相对 URI 为绝对 URI.
    /// </summary>
    /// <param name="baseUri">基础 URI.</param>
    /// <param name="relativeUri">相对 URI 字符串.</param>
    /// <returns>绝对 URI，如果解析失败则返回 null.</returns>
    public static Uri? ResolveUri(Uri baseUri, string? relativeUri)
    {
        if (string.IsNullOrWhiteSpace(relativeUri))
        {
            return null;
        }

        // 如果已经是绝对 URI，直接返回
        if (Uri.TryCreate(relativeUri, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        // 尝试相对于基础 URI 解析
        if (Uri.TryCreate(baseUri, relativeUri, out var resolvedUri))
        {
            return resolvedUri;
        }

        return null;
    }

    /// <summary>
    /// 解析相对 URI 为绝对 URI.
    /// </summary>
    /// <param name="baseUri">基础 URI.</param>
    /// <param name="relativeUri">相对 URI.</param>
    /// <returns>绝对 URI，如果解析失败则返回 null.</returns>
    public static Uri? ResolveUri(Uri baseUri, Uri? relativeUri)
    {
        if (relativeUri == null)
        {
            return null;
        }

        if (relativeUri.IsAbsoluteUri)
        {
            return relativeUri;
        }

        if (Uri.TryCreate(baseUri, relativeUri, out var resolvedUri))
        {
            return resolvedUri;
        }

        return null;
    }

    /// <summary>
    /// 获取基础 URI（去除查询字符串和片段）.
    /// </summary>
    /// <param name="uri">原始 URI.</param>
    /// <returns>基础 URI.</returns>
    public static Uri GetBaseUri(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
        {
            return uri;
        }

        var builder = new UriBuilder(uri)
        {
            Query = string.Empty,
            Fragment = string.Empty,
        };

        // 移除路径中的文件名部分，只保留目录
        var path = builder.Path;
        var lastSlash = path.LastIndexOf('/');
        if (lastSlash > 0)
        {
            builder.Path = path[..(lastSlash + 1)];
        }

        return builder.Uri;
    }
}

// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.BookScraper.Internal;

/// <summary>
/// URL 处理辅助类.
/// </summary>
internal static class UrlHelper
{
    /// <summary>
    /// 确保 URL 有协议前缀.
    /// </summary>
    /// <param name="url">原始 URL.</param>
    /// <param name="defaultScheme">默认协议（默认为 https）.</param>
    /// <returns>带协议的完整 URL.</returns>
    public static string? EnsureScheme(string? url, string defaultScheme = "https")
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        url = url.Trim();

        if (url.StartsWith("//", StringComparison.Ordinal))
        {
            return $"{defaultScheme}:{url}";
        }

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return $"{defaultScheme}://{url}";
        }

        return url;
    }

    /// <summary>
    /// 从 URL 中提取最后一个路径段.
    /// </summary>
    /// <param name="url">URL.</param>
    /// <returns>最后一个路径段，失败返回 null.</returns>
    public static string? ExtractLastSegment(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        try
        {
            var uri = new Uri(url);
            return uri.Segments.LastOrDefault()?.Trim('/');
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 从 URL 查询参数中提取指定参数的值.
    /// </summary>
    /// <param name="url">URL.</param>
    /// <param name="paramName">参数名.</param>
    /// <returns>参数值，不存在返回 null.</returns>
    public static string? ExtractQueryParam(string? url, string paramName)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        try
        {
            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            return query.Get(paramName);
        }
        catch
        {
            return null;
        }
    }
}

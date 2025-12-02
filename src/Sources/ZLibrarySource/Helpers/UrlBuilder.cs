// Copyright (c) Richasy. All rights reserved.

using System.Text;

namespace Richasy.RodelReader.Sources.ZLibrary.Helpers;

/// <summary>
/// URL 构建辅助类.
/// </summary>
internal static class UrlBuilder
{
    /// <summary>
    /// 构建搜索 URL.
    /// </summary>
    /// <param name="mirror">镜像地址.</param>
    /// <param name="query">搜索关键词.</param>
    /// <param name="page">页码.</param>
    /// <param name="options">搜索选项.</param>
    /// <returns>搜索 URL.</returns>
    public static string BuildSearchUrl(string mirror, string query, int page, BookSearchOptions? options)
    {
        var sb = new StringBuilder();
        sb.Append(mirror);
        sb.Append("/s/");
        sb.Append(HttpUtility.UrlEncode(query));
        sb.Append('?');

        if (options != null)
        {
            if (options.Exact)
            {
                sb.Append("e=1&");
            }

            if (options.FromYear.HasValue)
            {
                sb.Append($"yearFrom={options.FromYear.Value}&");
            }

            if (options.ToYear.HasValue)
            {
                sb.Append($"yearTo={options.ToYear.Value}&");
            }

            AppendLanguages(sb, options.Languages);
            AppendExtensions(sb, options.Extensions);
        }

        sb.Append($"page={page}");
        return sb.ToString();
    }

    /// <summary>
    /// 构建全文搜索 URL.
    /// </summary>
    /// <param name="mirror">镜像地址.</param>
    /// <param name="query">搜索关键词.</param>
    /// <param name="page">页码.</param>
    /// <param name="options">搜索选项.</param>
    /// <returns>全文搜索 URL.</returns>
    public static string BuildFullTextSearchUrl(string mirror, string query, int page, FullTextSearchOptions? options)
    {
        var sb = new StringBuilder();
        sb.Append(mirror);
        sb.Append("/fulltext/");
        sb.Append(HttpUtility.UrlEncode(query));
        sb.Append('?');

        if (options != null)
        {
            if (options.MatchPhrase)
            {
                sb.Append("type=phrase&");
            }
            else if (options.MatchWords)
            {
                sb.Append("type=words&");
            }

            if (options.Exact)
            {
                sb.Append("e=1&");
            }

            if (options.FromYear.HasValue)
            {
                sb.Append($"yearFrom={options.FromYear.Value}&");
            }

            if (options.ToYear.HasValue)
            {
                sb.Append($"yearTo={options.ToYear.Value}&");
            }

            AppendLanguages(sb, options.Languages);
            AppendExtensions(sb, options.Extensions);
        }

        sb.Append($"page={page}");
        return sb.ToString();
    }

    /// <summary>
    /// 构建书单搜索 URL.
    /// </summary>
    /// <param name="mirror">镜像地址.</param>
    /// <param name="query">搜索关键词.</param>
    /// <param name="page">页码.</param>
    /// <param name="order">排序方式.</param>
    /// <param name="isPrivate">是否私有书单.</param>
    /// <returns>书单搜索 URL.</returns>
    public static string BuildBooklistSearchUrl(string mirror, string query, int page, SortOrder order, bool isPrivate)
    {
        var orderValue = order switch
        {
            SortOrder.Newest => "date_created",
            SortOrder.Recent => "date_updated",
            _ => "popular",
        };

        var path = isPrivate ? "booklists/my" : "booklists";
        return $"{mirror}/{path}?searchQuery={HttpUtility.UrlEncode(query)}&order={orderValue}&page={page}";
    }

    /// <summary>
    /// 构建下载历史 URL.
    /// </summary>
    /// <param name="mirror">镜像地址.</param>
    /// <param name="page">页码.</param>
    /// <param name="fromDate">起始日期.</param>
    /// <param name="toDate">结束日期.</param>
    /// <returns>下载历史 URL.</returns>
    public static string BuildDownloadHistoryUrl(string mirror, int page, DateOnly? fromDate, DateOnly? toDate)
    {
        var dfrom = fromDate?.ToString("yy-MM-dd") ?? string.Empty;
        var dto = toDate?.ToString("yy-MM-dd") ?? string.Empty;
        return $"{mirror}/users/dstats.php?date_from={dfrom}&date_to={dto}&page={page}";
    }

    private static void AppendLanguages(StringBuilder sb, IReadOnlyList<BookLanguage>? languages)
    {
        if (languages == null || languages.Count == 0)
        {
            return;
        }

        foreach (var lang in languages)
        {
            sb.Append($"languages%5B%5D={lang.ToString().ToLowerInvariant()}&");
        }
    }

    private static void AppendExtensions(StringBuilder sb, IReadOnlyList<BookExtension>? extensions)
    {
        if (extensions == null || extensions.Count == 0)
        {
            return;
        }

        foreach (var ext in extensions)
        {
            sb.Append($"extensions%5B%5D={ext}&");
        }
    }
}

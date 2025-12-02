// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ApplePodcast.Helpers;

/// <summary>
/// URI 构建辅助类.
/// </summary>
internal static class UriHelper
{
    private const string TopPodcastsUrlTemplate = "https://itunes.apple.com/{0}/rss/toppodcasts/limit={1}/genre={2}/json";
    private const string LookupUrlTemplate = "https://itunes.apple.com/lookup?id={0}&entity=podcast";
    private const string SearchUrlTemplate = "https://itunes.apple.com/search?term={0}&media=podcast&entity=podcast&limit={1}";

    /// <summary>
    /// 构建热门播客 URL.
    /// </summary>
    /// <param name="region">区域代码.</param>
    /// <param name="limit">返回数量.</param>
    /// <param name="genreId">分类 ID.</param>
    /// <returns>请求 URI.</returns>
    public static Uri BuildTopPodcastsUri(string region, int limit, string genreId)
    {
        var url = string.Format(TopPodcastsUrlTemplate, region, limit, genreId);
        return new Uri(url);
    }

    /// <summary>
    /// 构建查询 URL.
    /// </summary>
    /// <param name="podcastId">播客 ID.</param>
    /// <returns>请求 URI.</returns>
    public static Uri BuildLookupUri(string podcastId)
    {
        var url = string.Format(LookupUrlTemplate, podcastId);
        return new Uri(url);
    }

    /// <summary>
    /// 构建搜索 URL.
    /// </summary>
    /// <param name="keyword">搜索关键词.</param>
    /// <param name="limit">返回数量.</param>
    /// <returns>请求 URI.</returns>
    public static Uri BuildSearchUri(string keyword, int limit)
    {
        var encodedKeyword = Uri.EscapeDataString(keyword);
        var url = string.Format(SearchUrlTemplate, encodedKeyword, limit);
        return new Uri(url);
    }
}

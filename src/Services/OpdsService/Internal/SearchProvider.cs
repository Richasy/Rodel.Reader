// Copyright (c) Richasy. All rights reserved.

using System.Text.RegularExpressions;

namespace Richasy.RodelReader.Services.OpdsService.Internal;

/// <summary>
/// 搜索提供器实现.
/// </summary>
internal sealed partial class SearchProvider : ISearchProvider
{
    private readonly IOpdsDispatcher _dispatcher;
    private readonly IOpdsParser _parser;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="SearchProvider"/> 类的新实例.
    /// </summary>
    /// <param name="dispatcher">HTTP 分发器.</param>
    /// <param name="parser">解析器.</param>
    /// <param name="logger">日志器.</param>
    public SearchProvider(
        IOpdsDispatcher dispatcher,
        IOpdsParser parser,
        ILogger logger)
    {
        _dispatcher = Guard.NotNull(dispatcher);
        _parser = Guard.NotNull(parser);
        _logger = Guard.NotNull(logger);
    }

    /// <inheritdoc/>
    public Uri? GetSearchDescriptionUri(OpdsFeed feed)
    {
        Guard.NotNull(feed);

        var searchLink = feed.GetSearchLink();
        if (searchLink == null)
        {
            _logger.LogDebug("No search link found in feed '{Title}'", feed.Title);
            return null;
        }

        // 检查是否是 OpenSearch 描述文档
        if (searchLink.MediaType?.Contains("application/opensearchdescription+xml", StringComparison.OrdinalIgnoreCase) == true)
        {
            return searchLink.Href;
        }

        // 如果是直接的搜索模板 URL，返回 null（需要使用 GetSearchTemplateAsync）
        _logger.LogDebug("Search link is not OpenSearch description document");
        return null;
    }

    /// <inheritdoc/>
    public async Task<string?> GetSearchTemplateAsync(OpdsFeed feed, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(feed);

        var searchLink = feed.GetSearchLink();
        if (searchLink == null)
        {
            _logger.LogDebug("No search link found in feed '{Title}'", feed.Title);
            return null;
        }

        // 检查是否是 OpenSearch 描述文档
        if (searchLink.MediaType?.Contains("application/opensearchdescription+xml", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogDebug("Fetching OpenSearch description from {Uri}", searchLink.Href);

            var stream = await _dispatcher.GetStreamAsync(searchLink.Href, cancellationToken).ConfigureAwait(false);
            var template = await _parser.ParseOpenSearchDescriptionAsync(stream, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(template))
            {
                _logger.LogDebug("Found search template: {Template}", template);
                return template;
            }

            _logger.LogWarning("OpenSearch description document did not contain a valid search template");
            return null;
        }

        // 如果是直接的 Atom 搜索链接，URL 本身可能包含搜索模板
        if (searchLink.MediaType?.Contains("application/atom+xml", StringComparison.OrdinalIgnoreCase) == true)
        {
            var template = searchLink.Href.ToString();
            if (template.Contains("{searchTerms}", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Search link URL contains search template: {Template}", template);
                return template;
            }
        }

        _logger.LogDebug("Could not determine search template from feed");
        return null;
    }

    /// <inheritdoc/>
    public Uri BuildSearchUri(string searchTemplate, string query)
    {
        Guard.NotNullOrWhiteSpace(searchTemplate);
        Guard.NotNullOrWhiteSpace(query);

        // 替换 OpenSearch 模板参数
        var url = SearchTermsRegex().Replace(searchTemplate, Uri.EscapeDataString(query));

        // 替换其他可选参数为空或默认值
        url = StartPageRegex().Replace(url, "1");
        url = StartIndexRegex().Replace(url, "1");
        url = CountRegex().Replace(url, "20");
        url = LanguageRegex().Replace(url, string.Empty);
        url = InputEncodingRegex().Replace(url, "UTF-8");
        url = OutputEncodingRegex().Replace(url, "UTF-8");

        // 清理空的查询参数
        url = EmptyParamRegex().Replace(url, string.Empty);
        url = url.TrimEnd('&', '?');

        _logger.LogDebug("Built search URI: {Uri}", url);

        return new Uri(url);
    }

    /// <inheritdoc/>
    public async Task<OpdsFeed> SearchAsync(string searchTemplate, string query, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(searchTemplate);
        Guard.NotNullOrWhiteSpace(query);

        var searchUri = BuildSearchUri(searchTemplate, query);

        _logger.LogDebug("Searching with query '{Query}' at {Uri}", query, searchUri);

        var stream = await _dispatcher.GetStreamAsync(searchUri, cancellationToken).ConfigureAwait(false);
        var feed = await _parser.ParseFeedAsync(stream, searchUri, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Search for '{Query}' returned {EntryCount} results", query, feed.Entries.Count);

        return feed;
    }

    /// <inheritdoc/>
    public async Task<OpdsFeed> SearchAsync(OpdsFeed feed, string query, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(feed);
        Guard.NotNullOrWhiteSpace(query);

        var template = await GetSearchTemplateAsync(feed, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(template))
        {
            throw new OpdsException("This feed does not support search");
        }

        return await SearchAsync(template, query, cancellationToken).ConfigureAwait(false);
    }

    [GeneratedRegex(@"\{searchTerms\??}", RegexOptions.IgnoreCase)]
    private static partial Regex SearchTermsRegex();

    [GeneratedRegex(@"\{startPage\??}", RegexOptions.IgnoreCase)]
    private static partial Regex StartPageRegex();

    [GeneratedRegex(@"\{startIndex\??}", RegexOptions.IgnoreCase)]
    private static partial Regex StartIndexRegex();

    [GeneratedRegex(@"\{count\??}", RegexOptions.IgnoreCase)]
    private static partial Regex CountRegex();

    [GeneratedRegex(@"\{language\??}", RegexOptions.IgnoreCase)]
    private static partial Regex LanguageRegex();

    [GeneratedRegex(@"\{inputEncoding\??}", RegexOptions.IgnoreCase)]
    private static partial Regex InputEncodingRegex();

    [GeneratedRegex(@"\{outputEncoding\??}", RegexOptions.IgnoreCase)]
    private static partial Regex OutputEncodingRegex();

    [GeneratedRegex(@"[&?][^&?=]+=(?=&|$)")]
    private static partial Regex EmptyParamRegex();
}

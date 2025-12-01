// Copyright (c) Richasy. All rights reserved.

using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 搜索操作实现.
/// </summary>
internal sealed class SearchOperator : ISearchOperator
{
    private readonly IWebDavDispatcher _dispatcher;
    private readonly IResponseParser<PropfindResponse> _propfindParser;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="SearchOperator"/> 类的新实例.
    /// </summary>
    public SearchOperator(
        IWebDavDispatcher dispatcher,
        IResponseParser<PropfindResponse> propfindParser,
        ILogger? logger = null)
    {
        _dispatcher = dispatcher;
        _propfindParser = propfindParser;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc/>
    public async Task<PropfindResponse> SearchAsync(Uri requestUri, SearchParameters parameters)
    {
        Guard.NotNull(requestUri);
        Guard.NotNull(parameters);
        parameters.Validate();

        _logger.LogDebug("Executing SEARCH on {Uri} with keyword '{Keyword}'", requestUri, parameters.Keyword);

        var headers = new HeaderBuilder()
            .AddWithOverwrite(parameters.Headers)
            .Build();

        var requestBody = SearchRequestBuilder.BuildRequestBody(parameters);
        var content = new StringContent(requestBody, Encoding.UTF8);

        try
        {
            var response = await _dispatcher.SendAsync(
                requestUri,
                WebDavMethod.Search,
                headers,
                content,
                new MediaTypeHeaderValue(WebDavConstants.MediaTypes.Xml),
                HttpCompletionOption.ResponseContentRead,
                parameters.CancellationToken).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = _propfindParser.Parse(responseContent, (int)response.StatusCode, response.ReasonPhrase);

            _logger.LogInformation(
                "SEARCH on {Uri} completed with {Count} results",
                requestUri,
                result.Resources.Count);

            return result;
        }
        catch (Exception ex) when (ex is not WebDavException)
        {
            _logger.LogError(ex, "SEARCH on {Uri} failed", requestUri);
            throw new WebDavException($"SEARCH operation failed for {requestUri}", ex);
        }
    }
}

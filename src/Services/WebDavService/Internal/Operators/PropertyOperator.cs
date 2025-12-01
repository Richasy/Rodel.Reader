// Copyright (c) Richasy. All rights reserved.

using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 属性操作实现.
/// </summary>
internal sealed class PropertyOperator : IPropertyOperator
{
    private readonly IWebDavDispatcher _dispatcher;
    private readonly IResponseParser<PropfindResponse> _propfindParser;
    private readonly IResponseParser<ProppatchResponse> _proppatchParser;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="PropertyOperator"/> 类的新实例.
    /// </summary>
    public PropertyOperator(
        IWebDavDispatcher dispatcher,
        IResponseParser<PropfindResponse> propfindParser,
        IResponseParser<ProppatchResponse> proppatchParser,
        ILogger? logger = null)
    {
        _dispatcher = dispatcher;
        _propfindParser = propfindParser;
        _proppatchParser = proppatchParser;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc/>
    public async Task<PropfindResponse> PropfindAsync(Uri requestUri, PropfindParameters? parameters = null)
    {
        Guard.NotNull(requestUri);
        parameters ??= new PropfindParameters();

        _logger.LogDebug("Executing PROPFIND on {Uri}", requestUri);

        var applyTo = parameters.ApplyTo ?? ApplyTo.Propfind.ResourceAndChildren;
        var headers = new HeaderBuilder()
            .Add(WebDavConstants.Headers.Depth, DepthHeaderHelper.GetValueForPropfind(applyTo))
            .AddWithOverwrite(parameters.Headers)
            .Build();

        HttpContent? content = null;
        if (parameters.RequestType != PropfindRequestType.AllPropertiesImplied)
        {
            var requestBody = PropfindRequestBuilder.BuildRequest(
                parameters.RequestType,
                parameters.CustomProperties,
                parameters.Namespaces);
            content = new StringContent(requestBody, Encoding.UTF8);
        }

        try
        {
            var response = await _dispatcher.SendAsync(
                requestUri,
                WebDavMethod.Propfind,
                headers,
                content,
                parameters.ContentType ?? new MediaTypeHeaderValue(WebDavConstants.MediaTypes.Xml),
                HttpCompletionOption.ResponseContentRead,
                parameters.CancellationToken).ConfigureAwait(false);

            var responseContent = await ReadResponseContentAsync(response).ConfigureAwait(false);
            var result = _propfindParser.Parse(responseContent, (int)response.StatusCode, response.ReasonPhrase);

            _logger.LogInformation(
                "PROPFIND on {Uri} completed with {Count} resources",
                requestUri,
                result.Resources.Count);

            return result;
        }
        catch (Exception ex) when (ex is not WebDavException)
        {
            _logger.LogError(ex, "PROPFIND on {Uri} failed", requestUri);
            throw new WebDavException($"PROPFIND operation failed for {requestUri}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<ProppatchResponse> ProppatchAsync(Uri requestUri, ProppatchParameters parameters)
    {
        Guard.NotNull(requestUri);
        Guard.NotNull(parameters);

        _logger.LogDebug("Executing PROPPATCH on {Uri}", requestUri);

        var headerBuilder = new HeaderBuilder();
        if (!string.IsNullOrEmpty(parameters.LockToken))
        {
            headerBuilder.Add(WebDavConstants.Headers.If, IfHeaderHelper.GetHeaderValue(parameters.LockToken));
        }

        var headers = headerBuilder.AddWithOverwrite(parameters.Headers).Build();

        var requestBody = ProppatchRequestBuilder.BuildRequestBody(
            parameters.PropertiesToSet,
            parameters.PropertiesToRemove,
            parameters.Namespaces);
        var content = new StringContent(requestBody, Encoding.UTF8);

        try
        {
            var response = await _dispatcher.SendAsync(
                requestUri,
                WebDavMethod.Proppatch,
                headers,
                content,
                parameters.ContentType ?? new MediaTypeHeaderValue(WebDavConstants.MediaTypes.Xml),
                HttpCompletionOption.ResponseContentRead,
                parameters.CancellationToken).ConfigureAwait(false);

            var responseContent = await ReadResponseContentAsync(response).ConfigureAwait(false);
            var result = _proppatchParser.Parse(responseContent, (int)response.StatusCode, response.ReasonPhrase);

            _logger.LogInformation("PROPPATCH on {Uri} completed", requestUri);
            return result;
        }
        catch (Exception ex) when (ex is not WebDavException)
        {
            _logger.LogError(ex, "PROPPATCH on {Uri} failed", requestUri);
            throw new WebDavException($"PROPPATCH operation failed for {requestUri}", ex);
        }
    }

    private static async Task<string> ReadResponseContentAsync(HttpResponseMessage response)
    {
        var data = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        var encoding = GetResponseEncoding(response.Content, Encoding.UTF8);
        return encoding.GetString(data, 0, data.Length);
    }

    private static Encoding GetResponseEncoding(HttpContent content, Encoding fallback)
    {
        var charset = content.Headers.ContentType?.CharSet;
        if (string.IsNullOrEmpty(charset))
        {
            return fallback;
        }

        try
        {
            return Encoding.GetEncoding(charset);
        }
        catch (ArgumentException)
        {
            return fallback;
        }
    }
}

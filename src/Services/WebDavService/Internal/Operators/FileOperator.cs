// Copyright (c) Richasy. All rights reserved.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 文件操作实现.
/// </summary>
internal sealed class FileOperator : IFileOperator
{
    private readonly IWebDavDispatcher _dispatcher;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="FileOperator"/> 类的新实例.
    /// </summary>
    public FileOperator(IWebDavDispatcher dispatcher, ILogger? logger = null)
    {
        _dispatcher = dispatcher;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc/>
    public Task<WebDavStreamResponse> GetRawFileAsync(Uri requestUri, GetFileParameters? parameters = null)
    {
        return GetFileAsync(requestUri, translate: false, parameters);
    }

    /// <inheritdoc/>
    public Task<WebDavStreamResponse> GetProcessedFileAsync(Uri requestUri, GetFileParameters? parameters = null)
    {
        return GetFileAsync(requestUri, translate: true, parameters);
    }

    /// <inheritdoc/>
    public Task<WebDavResponse> PutFileAsync(Uri requestUri, Stream stream, PutFileParameters? parameters = null)
    {
        var content = new StreamContent(stream);
        return PutFileAsync(requestUri, content, parameters);
    }

    /// <inheritdoc/>
    public async Task<WebDavResponse> PutFileAsync(Uri requestUri, HttpContent content, PutFileParameters? parameters = null)
    {
        Guard.NotNull(requestUri);
        Guard.NotNull(content);
        parameters ??= new PutFileParameters();

        _logger.LogDebug("Executing PUT on {Uri}", requestUri);

        var headerBuilder = new HeaderBuilder();
        if (!string.IsNullOrEmpty(parameters.LockToken))
        {
            headerBuilder.Add(WebDavConstants.Headers.If, IfHeaderHelper.GetHeaderValue(parameters.LockToken));
        }

        var headers = headerBuilder.AddWithOverwrite(parameters.Headers).Build();

        try
        {
            var response = await _dispatcher.SendAsync(
                requestUri,
                HttpMethod.Put,
                headers,
                content,
                parameters.ContentType,
                HttpCompletionOption.ResponseContentRead,
                parameters.CancellationToken).ConfigureAwait(false);

            var result = new WebDavResponse((int)response.StatusCode, response.ReasonPhrase);
            _logger.LogInformation("PUT on {Uri} completed with status {StatusCode}", requestUri, result.StatusCode);
            return result;
        }
        catch (Exception ex) when (ex is not WebDavException)
        {
            _logger.LogError(ex, "PUT on {Uri} failed", requestUri);
            throw new WebDavException($"PUT operation failed for {requestUri}", ex);
        }
    }

    private async Task<WebDavStreamResponse> GetFileAsync(Uri requestUri, bool translate, GetFileParameters? parameters)
    {
        Guard.NotNull(requestUri);
        parameters ??= new GetFileParameters();

        _logger.LogDebug("Executing GET on {Uri} (translate={Translate})", requestUri, translate);

        var headers = new HeaderBuilder()
            .Add(WebDavConstants.Headers.Translate, translate ? "t" : "f")
            .AddWithOverwrite(parameters.Headers)
            .Build();

        try
        {
            var response = await _dispatcher.SendAsync(
                requestUri,
                HttpMethod.Get,
                headers,
                cancellationToken: parameters.CancellationToken,
                completionOption: HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "GET on {Uri} returned {StatusCode}",
                    requestUri,
                    (int)response.StatusCode);
                return new WebDavStreamResponse((int)response.StatusCode, response.ReasonPhrase);
            }

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            _logger.LogInformation(
                "GET on {Uri} completed, ContentLength={ContentLength}",
                requestUri,
                response.Content.Headers.ContentLength);

            return new WebDavStreamResponse(response, stream);
        }
        catch (Exception ex) when (ex is not WebDavException)
        {
            _logger.LogError(ex, "GET on {Uri} failed", requestUri);
            throw new WebDavException($"GET operation failed for {requestUri}", ex);
        }
    }
}

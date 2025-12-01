// Copyright (c) Richasy. All rights reserved.

using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 锁操作实现.
/// </summary>
internal sealed class LockOperator : ILockOperator
{
    private readonly IWebDavDispatcher _dispatcher;
    private readonly IResponseParser<LockResponse> _lockParser;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="LockOperator"/> 类的新实例.
    /// </summary>
    public LockOperator(
        IWebDavDispatcher dispatcher,
        IResponseParser<LockResponse> lockParser,
        ILogger? logger = null)
    {
        _dispatcher = dispatcher;
        _lockParser = lockParser;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc/>
    public async Task<LockResponse> LockAsync(Uri requestUri, LockParameters? parameters = null)
    {
        Guard.NotNull(requestUri);
        parameters ??= new LockParameters();

        _logger.LogDebug("Executing LOCK on {Uri}", requestUri);

        var headerBuilder = new HeaderBuilder();

        if (parameters.ApplyTo.HasValue)
        {
            headerBuilder.Add(WebDavConstants.Headers.Depth, DepthHeaderHelper.GetValueForLock(parameters.ApplyTo.Value));
        }

        if (parameters.Timeout.HasValue)
        {
            headerBuilder.Add(WebDavConstants.Headers.Timeout, $"Second-{parameters.Timeout.Value.TotalSeconds:F0}");
        }

        var headers = headerBuilder.AddWithOverwrite(parameters.Headers).Build();
        var requestBody = LockRequestBuilder.BuildRequestBody(parameters);
        var content = new StringContent(requestBody, Encoding.UTF8);

        try
        {
            var response = await _dispatcher.SendAsync(
                requestUri,
                WebDavMethod.Lock,
                headers,
                content,
                parameters.ContentType ?? new MediaTypeHeaderValue(WebDavConstants.MediaTypes.Xml),
                HttpCompletionOption.ResponseContentRead,
                parameters.CancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("LOCK on {Uri} failed with status {StatusCode}", requestUri, (int)response.StatusCode);
                return new LockResponse((int)response.StatusCode, response.ReasonPhrase);
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = _lockParser.Parse(responseContent, (int)response.StatusCode, response.ReasonPhrase);

            _logger.LogInformation("LOCK on {Uri} completed, LockToken={LockToken}", requestUri, result.LockToken);
            return result;
        }
        catch (Exception ex) when (ex is not WebDavException)
        {
            _logger.LogError(ex, "LOCK on {Uri} failed", requestUri);
            throw new WebDavException($"LOCK operation failed for {requestUri}", ex);
        }
    }

    /// <inheritdoc/>
    public Task<WebDavResponse> UnlockAsync(Uri requestUri, string lockToken)
    {
        return UnlockAsync(requestUri, new UnlockParameters(lockToken));
    }

    /// <inheritdoc/>
    public async Task<WebDavResponse> UnlockAsync(Uri requestUri, UnlockParameters parameters)
    {
        Guard.NotNull(requestUri);
        Guard.NotNull(parameters);
        Guard.NotNullOrEmpty(parameters.LockToken);

        _logger.LogDebug("Executing UNLOCK on {Uri} with token {LockToken}", requestUri, parameters.LockToken);

        var headers = new HeaderBuilder()
            .Add(WebDavConstants.Headers.LockToken, $"<{parameters.LockToken}>")
            .AddWithOverwrite(parameters.Headers)
            .Build();

        try
        {
            var response = await _dispatcher.SendAsync(
                requestUri,
                WebDavMethod.Unlock,
                headers,
                cancellationToken: parameters.CancellationToken).ConfigureAwait(false);

            var result = new WebDavResponse((int)response.StatusCode, response.ReasonPhrase);
            _logger.LogInformation("UNLOCK on {Uri} completed with status {StatusCode}", requestUri, result.StatusCode);
            return result;
        }
        catch (Exception ex) when (ex is not WebDavException)
        {
            _logger.LogError(ex, "UNLOCK on {Uri} failed", requestUri);
            throw new WebDavException($"UNLOCK operation failed for {requestUri}", ex);
        }
    }
}

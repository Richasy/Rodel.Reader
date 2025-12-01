// Copyright (c) Richasy. All rights reserved.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 资源操作实现.
/// </summary>
internal sealed class ResourceOperator : IResourceOperator
{
    private readonly IWebDavDispatcher _dispatcher;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="ResourceOperator"/> 类的新实例.
    /// </summary>
    public ResourceOperator(IWebDavDispatcher dispatcher, ILogger? logger = null)
    {
        _dispatcher = dispatcher;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc/>
    public async Task<WebDavResponse> MkColAsync(Uri requestUri, MkColParameters? parameters = null)
    {
        Guard.NotNull(requestUri);
        parameters ??= new MkColParameters();

        _logger.LogDebug("Executing MKCOL on {Uri}", requestUri);

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
                WebDavMethod.Mkcol,
                headers,
                cancellationToken: parameters.CancellationToken).ConfigureAwait(false);

            var result = new WebDavResponse((int)response.StatusCode, response.ReasonPhrase);
            _logger.LogInformation("MKCOL on {Uri} completed with status {StatusCode}", requestUri, result.StatusCode);
            return result;
        }
        catch (Exception ex) when (ex is not WebDavException)
        {
            _logger.LogError(ex, "MKCOL on {Uri} failed", requestUri);
            throw new WebDavException($"MKCOL operation failed for {requestUri}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<WebDavResponse> DeleteAsync(Uri requestUri, DeleteParameters? parameters = null)
    {
        Guard.NotNull(requestUri);
        parameters ??= new DeleteParameters();

        _logger.LogDebug("Executing DELETE on {Uri}", requestUri);

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
                HttpMethod.Delete,
                headers,
                cancellationToken: parameters.CancellationToken).ConfigureAwait(false);

            var result = new WebDavResponse((int)response.StatusCode, response.ReasonPhrase);
            _logger.LogInformation("DELETE on {Uri} completed with status {StatusCode}", requestUri, result.StatusCode);
            return result;
        }
        catch (Exception ex) when (ex is not WebDavException)
        {
            _logger.LogError(ex, "DELETE on {Uri} failed", requestUri);
            throw new WebDavException($"DELETE operation failed for {requestUri}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<WebDavResponse> CopyAsync(Uri sourceUri, Uri destUri, CopyParameters? parameters = null)
    {
        Guard.NotNull(sourceUri);
        Guard.NotNull(destUri);
        parameters ??= new CopyParameters();

        _logger.LogDebug("Executing COPY from {Source} to {Dest}", sourceUri, destUri);

        var applyTo = parameters.ApplyTo ?? ApplyTo.Copy.ResourceAndAncestors;
        var headerBuilder = new HeaderBuilder()
            .Add(WebDavConstants.Headers.Destination, GetAbsoluteUri(destUri).AbsoluteUri)
            .Add(WebDavConstants.Headers.Depth, DepthHeaderHelper.GetValueForCopy(applyTo))
            .Add(WebDavConstants.Headers.Overwrite, parameters.Overwrite ? "T" : "F");

        if (!string.IsNullOrEmpty(parameters.DestLockToken))
        {
            headerBuilder.Add(WebDavConstants.Headers.If, IfHeaderHelper.GetHeaderValue(parameters.DestLockToken));
        }

        var headers = headerBuilder.AddWithOverwrite(parameters.Headers).Build();

        try
        {
            var response = await _dispatcher.SendAsync(
                sourceUri,
                WebDavMethod.Copy,
                headers,
                cancellationToken: parameters.CancellationToken).ConfigureAwait(false);

            var result = new WebDavResponse((int)response.StatusCode, response.ReasonPhrase);
            _logger.LogInformation("COPY from {Source} to {Dest} completed with status {StatusCode}", sourceUri, destUri, result.StatusCode);
            return result;
        }
        catch (Exception ex) when (ex is not WebDavException)
        {
            _logger.LogError(ex, "COPY from {Source} to {Dest} failed", sourceUri, destUri);
            throw new WebDavException($"COPY operation failed from {sourceUri} to {destUri}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<WebDavResponse> MoveAsync(Uri sourceUri, Uri destUri, MoveParameters? parameters = null)
    {
        Guard.NotNull(sourceUri);
        Guard.NotNull(destUri);
        parameters ??= new MoveParameters();

        _logger.LogDebug("Executing MOVE from {Source} to {Dest}", sourceUri, destUri);

        var headerBuilder = new HeaderBuilder()
            .Add(WebDavConstants.Headers.Destination, GetAbsoluteUri(destUri).AbsoluteUri)
            .Add(WebDavConstants.Headers.Overwrite, parameters.Overwrite ? "T" : "F");

        if (!string.IsNullOrEmpty(parameters.SourceLockToken))
        {
            headerBuilder.Add(WebDavConstants.Headers.If, IfHeaderHelper.GetHeaderValue(parameters.SourceLockToken));
        }

        if (!string.IsNullOrEmpty(parameters.DestLockToken))
        {
            headerBuilder.Add(WebDavConstants.Headers.If, IfHeaderHelper.GetHeaderValue(parameters.DestLockToken));
        }

        var headers = headerBuilder.AddWithOverwrite(parameters.Headers).Build();

        try
        {
            var response = await _dispatcher.SendAsync(
                sourceUri,
                WebDavMethod.Move,
                headers,
                cancellationToken: parameters.CancellationToken).ConfigureAwait(false);

            var result = new WebDavResponse((int)response.StatusCode, response.ReasonPhrase);
            _logger.LogInformation("MOVE from {Source} to {Dest} completed with status {StatusCode}", sourceUri, destUri, result.StatusCode);
            return result;
        }
        catch (Exception ex) when (ex is not WebDavException)
        {
            _logger.LogError(ex, "MOVE from {Source} to {Dest} failed", sourceUri, destUri);
            throw new WebDavException($"MOVE operation failed from {sourceUri} to {destUri}", ex);
        }
    }

    private Uri GetAbsoluteUri(Uri uri)
    {
        if (uri.IsAbsoluteUri)
        {
            return uri;
        }

        if (_dispatcher.BaseAddress == null)
        {
            throw new InvalidOperationException(
                "The URI must be an absolute URI or BaseAddress must be set.");
        }

        return new Uri(_dispatcher.BaseAddress, uri);
    }
}

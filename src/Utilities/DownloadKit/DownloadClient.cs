// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.DownloadKit;

/// <summary>
/// 下载客户端，提供文件下载功能.
/// </summary>
/// <remarks>
/// 这是一个轻量级、AOT 友好的下载客户端，支持：
/// <list type="bullet">
/// <item>HTTP/HTTPS 下载</item>
/// <item>自定义请求头</item>
/// <item>实时进度报告（带节流）</item>
/// <item>下载速度计算</item>
/// <item>取消支持</item>
/// </list>
/// </remarks>
public sealed class DownloadClient : IDownloadClient
{
    private readonly HttpClient _httpClient;
    private readonly HttpDownloader _downloader;
    private readonly ILogger<DownloadClient> _logger;
    private readonly bool _ownsHttpClient;
    private bool _disposed;

    /// <summary>
    /// 初始化 <see cref="DownloadClient"/> 类的新实例.
    /// </summary>
    /// <remarks>
    /// 使用此构造函数时，会创建一个内部的 <see cref="HttpClient"/> 实例，
    /// 并在 <see cref="Dispose"/> 时释放.
    /// </remarks>
    public DownloadClient()
        : this(CreateDefaultHttpClient(), null, ownsHttpClient: true)
    {
    }

    /// <summary>
    /// 初始化 <see cref="DownloadClient"/> 类的新实例.
    /// </summary>
    /// <param name="logger">日志器.</param>
    public DownloadClient(ILogger<DownloadClient>? logger)
        : this(CreateDefaultHttpClient(), logger, ownsHttpClient: true)
    {
    }

    /// <summary>
    /// 初始化 <see cref="DownloadClient"/> 类的新实例.
    /// </summary>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <remarks>
    /// 使用此构造函数时，调用方负责管理 <see cref="HttpClient"/> 的生命周期.
    /// </remarks>
    public DownloadClient(HttpClient httpClient)
        : this(httpClient, null, ownsHttpClient: false)
    {
    }

    /// <summary>
    /// 初始化 <see cref="DownloadClient"/> 类的新实例.
    /// </summary>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="logger">日志器.</param>
    public DownloadClient(HttpClient httpClient, ILogger<DownloadClient>? logger)
        : this(httpClient, logger, ownsHttpClient: false)
    {
    }

    private DownloadClient(HttpClient httpClient, ILogger<DownloadClient>? logger, bool ownsHttpClient)
    {
        _httpClient = Guard.NotNull(httpClient);
        _logger = logger ?? NullLogger<DownloadClient>.Instance;
        _ownsHttpClient = ownsHttpClient;
        _downloader = new HttpDownloader(_httpClient, _logger);

        _logger.LogDebug("DownloadClient 初始化完成");
    }

    /// <inheritdoc/>
    public Task<DownloadResult> DownloadAsync(
        string url,
        string destinationPath,
        DownloadOptions? options = null,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(url);
        return DownloadAsync(new Uri(url), destinationPath, options, progress, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<DownloadResult> DownloadAsync(
        Uri uri,
        string destinationPath,
        DownloadOptions? options = null,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.NotNull(uri);
        Guard.NotNullOrWhiteSpace(destinationPath);

        var effectiveOptions = options ?? DownloadOptions.Default;

        _logger.LogDebug("开始下载任务: {Uri} -> {Path}", uri, destinationPath);

        return _downloader.DownloadAsync(uri, destinationPath, effectiveOptions, progress, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<RemoteFileInfo> GetFileInfoAsync(
        string url,
        DownloadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.NotNullOrWhiteSpace(url);

        var uri = new Uri(url);
        var effectiveOptions = options ?? DownloadOptions.Default;

        return await _downloader.GetFileInfoAsync(uri, effectiveOptions, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 创建下载客户端的快捷方法.
    /// </summary>
    /// <returns>新的下载客户端实例.</returns>
    public static DownloadClient Create() => new();

    /// <summary>
    /// 创建下载客户端的快捷方法.
    /// </summary>
    /// <param name="logger">日志器.</param>
    /// <returns>新的下载客户端实例.</returns>
    public static DownloadClient Create(ILogger<DownloadClient> logger) => new(logger);

    /// <summary>
    /// 创建下载客户端的快捷方法.
    /// </summary>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="logger">日志器.</param>
    /// <returns>新的下载客户端实例.</returns>
    public static DownloadClient Create(HttpClient httpClient, ILogger<DownloadClient>? logger = null) => new(httpClient, logger);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
            _logger.LogDebug("已释放内部 HttpClient");
        }

        _logger.LogDebug("DownloadClient 已释放");
    }

    private static HttpClient CreateDefaultHttpClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10,
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMinutes(30), // 默认 30 分钟超时
        };

        client.DefaultRequestHeaders.Add("Accept", "*/*");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        return client;
    }
}

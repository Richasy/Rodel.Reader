// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Podcast.Apple;

/// <summary>
/// Apple Podcast 客户端实现.
/// </summary>
public sealed class ApplePodcastClient : IApplePodcastClient
{
    private const string DefaultAccept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
    private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

    private readonly IPodcastDispatcher _dispatcher;
    private readonly ILogger<ApplePodcastClient> _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplePodcastClient"/> class.
    /// </summary>
    public ApplePodcastClient()
        : this(new ApplePodcastClientOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplePodcastClient"/> class.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    public ApplePodcastClient(ApplePodcastClientOptions options)
        : this(options, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplePodcastClient"/> class.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="logger">日志器.</param>
    public ApplePodcastClient(ApplePodcastClientOptions options, ILogger<ApplePodcastClient>? logger)
        : this(options, CreateHttpClient(options), logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplePodcastClient"/> class.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="logger">日志器.</param>
    public ApplePodcastClient(ApplePodcastClientOptions options, HttpClient httpClient, ILogger<ApplePodcastClient>? logger)
    {
        Guard.NotNull(options);
        Guard.NotNull(httpClient);

        Options = options;
        _logger = logger ?? NullLogger<ApplePodcastClient>.Instance;
        _logger.LogDebug("Initializing ApplePodcastClient");

        // 配置 HttpClient
        ConfigureHttpClient(httpClient, options);

        // 创建分发器
        _dispatcher = new PodcastDispatcher(httpClient, _logger);

        // 创建解析器
        var parser = new PodcastFeedParser(_logger);

        // 创建各操作模块
        Categories = new CategoryProvider(_dispatcher, options, _logger);
        Search = new PodcastSearcher(_dispatcher, options, _logger);
        Details = new PodcastDetailProvider(_dispatcher, parser, _logger);

        _logger.LogInformation("ApplePodcastClient initialized successfully");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplePodcastClient"/> class.
    /// 完全依赖注入构造函数.
    /// </summary>
    /// <param name="categories">分类提供器.</param>
    /// <param name="search">搜索器.</param>
    /// <param name="details">详情提供器.</param>
    /// <param name="dispatcher">HTTP 分发器.</param>
    /// <param name="options">客户端配置.</param>
    /// <param name="logger">日志器.</param>
    internal ApplePodcastClient(
        ICategoryProvider categories,
        IPodcastSearcher search,
        IPodcastDetailProvider details,
        IPodcastDispatcher dispatcher,
        ApplePodcastClientOptions options,
        ILogger<ApplePodcastClient>? logger = null)
    {
        Categories = Guard.NotNull(categories);
        Search = Guard.NotNull(search);
        Details = Guard.NotNull(details);
        _dispatcher = Guard.NotNull(dispatcher);
        Options = Guard.NotNull(options);
        _logger = logger ?? NullLogger<ApplePodcastClient>.Instance;
    }

    /// <inheritdoc/>
    public ICategoryProvider Categories { get; }

    /// <inheritdoc/>
    public IPodcastSearcher Search { get; }

    /// <inheritdoc/>
    public IPodcastDetailProvider Details { get; }

    /// <inheritdoc/>
    public ApplePodcastClientOptions Options { get; }

    /// <summary>
    /// 创建 Apple Podcast 客户端.
    /// </summary>
    /// <returns>新的客户端实例.</returns>
    public static ApplePodcastClient Create()
    {
        return new ApplePodcastClient();
    }

    /// <summary>
    /// 创建 Apple Podcast 客户端.
    /// </summary>
    /// <param name="region">默认区域代码.</param>
    /// <returns>新的客户端实例.</returns>
    public static ApplePodcastClient Create(string region)
    {
        return new ApplePodcastClient(new ApplePodcastClientOptions { DefaultRegion = region });
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _dispatcher.Dispose();
            _disposed = true;
            _logger.LogDebug("ApplePodcastClient disposed");
        }
    }

    private static HttpClient CreateHttpClient(ApplePodcastClientOptions options)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };

        var client = new HttpClient(handler)
        {
            Timeout = options.Timeout,
        };

        return client;
    }

    private static void ConfigureHttpClient(HttpClient httpClient, ApplePodcastClientOptions options)
    {
        httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };
        httpClient.DefaultRequestHeaders.Accept.ParseAdd(DefaultAccept);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent ?? DefaultUserAgent);

        if (httpClient.Timeout == Timeout.InfiniteTimeSpan)
        {
            httpClient.Timeout = options.Timeout;
        }
    }
}

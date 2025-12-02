// Copyright (c) Richasy. All rights reserved.

using System.Net.Http.Headers;

namespace Richasy.RodelReader.Services.OpdsService;

/// <summary>
/// OPDS 客户端实现.
/// </summary>
public sealed class OpdsClient : IOpdsClient
{
    private readonly IOpdsDispatcher _dispatcher;
    private readonly ILogger<OpdsClient> _logger;
    private bool _disposed;

    /// <summary>
    /// 初始化 <see cref="OpdsClient"/> 类的新实例.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    public OpdsClient(OpdsClientOptions options)
        : this(options, null)
    {
    }

    /// <summary>
    /// 初始化 <see cref="OpdsClient"/> 类的新实例.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="logger">日志器.</param>
    public OpdsClient(OpdsClientOptions options, ILogger<OpdsClient>? logger)
        : this(options, CreateHttpClient(options), logger)
    {
    }

    /// <summary>
    /// 初始化 <see cref="OpdsClient"/> 类的新实例.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="logger">日志器.</param>
    public OpdsClient(OpdsClientOptions options, HttpClient httpClient, ILogger<OpdsClient>? logger)
    {
        Guard.NotNull(options);
        Guard.NotNull(httpClient);

        Options = options;
        _logger = logger ?? NullLogger<OpdsClient>.Instance;
        _logger.LogDebug("Initializing OpdsClient with root URI: {RootUri}", options.RootUri);

        // 配置 HttpClient
        ConfigureHttpClient(httpClient, options);

        // 创建分发器
        _dispatcher = new OpdsDispatcher(httpClient, _logger);

        // 创建解析器
        var parser = new OpdsV1Parser(_logger);

        // 创建各操作模块
        Catalog = new CatalogNavigator(_dispatcher, parser, options, _logger);
        Search = new SearchProvider(_dispatcher, parser, _logger);

        _logger.LogInformation("OpdsClient initialized successfully");
    }

    /// <summary>
    /// 初始化 <see cref="OpdsClient"/> 类的新实例（完全依赖注入）.
    /// </summary>
    /// <param name="catalog">目录导航器.</param>
    /// <param name="search">搜索提供器.</param>
    /// <param name="dispatcher">HTTP 分发器.</param>
    /// <param name="options">客户端配置.</param>
    /// <param name="logger">日志器.</param>
    internal OpdsClient(
        ICatalogNavigator catalog,
        ISearchProvider search,
        IOpdsDispatcher dispatcher,
        OpdsClientOptions options,
        ILogger<OpdsClient>? logger = null)
    {
        Catalog = Guard.NotNull(catalog);
        Search = Guard.NotNull(search);
        _dispatcher = Guard.NotNull(dispatcher);
        Options = Guard.NotNull(options);
        _logger = logger ?? NullLogger<OpdsClient>.Instance;
    }

    /// <inheritdoc/>
    public ICatalogNavigator Catalog { get; }

    /// <inheritdoc/>
    public ISearchProvider Search { get; }

    /// <inheritdoc/>
    public OpdsClientOptions Options { get; }

    /// <summary>
    /// 创建 OPDS 客户端.
    /// </summary>
    /// <param name="rootUri">OPDS 根目录 URI.</param>
    /// <returns>OPDS 客户端实例.</returns>
    public static OpdsClient Create(Uri rootUri)
        => new(new OpdsClientOptions { RootUri = rootUri });

    /// <summary>
    /// 创建带认证的 OPDS 客户端.
    /// </summary>
    /// <param name="rootUri">OPDS 根目录 URI.</param>
    /// <param name="userName">用户名.</param>
    /// <param name="password">密码.</param>
    /// <returns>OPDS 客户端实例.</returns>
    public static OpdsClient Create(Uri rootUri, string? userName, string? password)
        => new(new OpdsClientOptions
        {
            RootUri = rootUri,
            Credentials = string.IsNullOrEmpty(userName) ? null : new NetworkCredential(userName, password),
        });

    /// <summary>
    /// 创建带认证和日志的 OPDS 客户端.
    /// </summary>
    /// <param name="rootUri">OPDS 根目录 URI.</param>
    /// <param name="userName">用户名.</param>
    /// <param name="password">密码.</param>
    /// <param name="logger">日志器.</param>
    /// <returns>OPDS 客户端实例.</returns>
    public static OpdsClient Create(Uri rootUri, string? userName, string? password, ILogger<OpdsClient> logger)
        => new(
            new OpdsClientOptions
            {
                RootUri = rootUri,
                Credentials = string.IsNullOrEmpty(userName) ? null : new NetworkCredential(userName, password),
            },
            logger);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _dispatcher.Dispose();
        _disposed = true;

        _logger.LogDebug("OpdsClient disposed");
    }

    private static HttpClient CreateHttpClient(OpdsClientOptions options)
    {
        var handler = new HttpClientHandler();

        if (options.Credentials != null)
        {
            handler.Credentials = options.Credentials;
            handler.PreAuthenticate = true;
        }

        return new HttpClient(handler);
    }

    private static void ConfigureHttpClient(HttpClient httpClient, OpdsClientOptions options)
    {
        httpClient.Timeout = options.Timeout;

        if (!string.IsNullOrEmpty(options.UserAgent))
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        }
        else
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RodelReader-OpdsClient/1.0");
        }

        // 设置 Accept 头
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/atom+xml"));
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml", 0.8));

        // 如果有凭据，设置 Basic 认证头
        if (options.Credentials != null)
        {
            var credentials = $"{options.Credentials.UserName}:{options.Credentials.Password}";
            var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
        }
    }
}

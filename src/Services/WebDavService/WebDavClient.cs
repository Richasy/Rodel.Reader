// Copyright (c) Richasy. All rights reserved.

using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 客户端实现.
/// </summary>
public sealed class WebDavClient : IWebDavClient
{
    private readonly IWebDavDispatcher _dispatcher;
    private readonly ILogger<WebDavClient> _logger;
    private bool _disposed;

    /// <summary>
    /// 初始化 <see cref="WebDavClient"/> 类的新实例.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    public WebDavClient(WebDavClientOptions options)
        : this(options, CreateHttpClient(options), null)
    {
    }

    /// <summary>
    /// 初始化 <see cref="WebDavClient"/> 类的新实例.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="logger">日志器.</param>
    public WebDavClient(WebDavClientOptions options, ILogger<WebDavClient>? logger)
        : this(options, CreateHttpClient(options), logger)
    {
    }

    /// <summary>
    /// 初始化 <see cref="WebDavClient"/> 类的新实例.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="logger">日志器.</param>
    public WebDavClient(WebDavClientOptions options, HttpClient httpClient, ILogger<WebDavClient>? logger)
    {
        Guard.NotNull(options);
        Guard.NotNull(httpClient);

        _logger = logger ?? NullLogger<WebDavClient>.Instance;
        _logger.LogDebug("Initializing WebDavClient with base address: {BaseAddress}", options.BaseAddress);

        // 配置 HttpClient
        ConfigureHttpClient(httpClient, options);

        // 创建分发器
        _dispatcher = new WebDavDispatcher(httpClient, _logger);

        // 创建解析器
        var propfindParser = new PropfindResponseParser(_logger);
        var proppatchParser = new ProppatchResponseParser(_logger);
        var lockParser = new LockResponseParser();

        // 创建各操作模块
        Properties = new PropertyOperator(_dispatcher, propfindParser, proppatchParser, _logger);
        Resources = new ResourceOperator(_dispatcher, _logger);
        Files = new FileOperator(_dispatcher, _logger);
        Locks = new LockOperator(_dispatcher, lockParser, _logger);
        Search = new SearchOperator(_dispatcher, propfindParser, _logger);

        _logger.LogInformation("WebDavClient initialized successfully");
    }

    /// <summary>
    /// 初始化 <see cref="WebDavClient"/> 类的新实例（完全依赖注入）.
    /// </summary>
    /// <param name="properties">属性操作器.</param>
    /// <param name="resources">资源操作器.</param>
    /// <param name="files">文件操作器.</param>
    /// <param name="locks">锁操作器.</param>
    /// <param name="search">搜索操作器.</param>
    /// <param name="dispatcher">HTTP 分发器.</param>
    /// <param name="logger">日志器.</param>
    public WebDavClient(
        IPropertyOperator properties,
        IResourceOperator resources,
        IFileOperator files,
        ILockOperator locks,
        ISearchOperator search,
        IWebDavDispatcher dispatcher,
        ILogger<WebDavClient>? logger = null)
    {
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        Resources = resources ?? throw new ArgumentNullException(nameof(resources));
        Files = files ?? throw new ArgumentNullException(nameof(files));
        Locks = locks ?? throw new ArgumentNullException(nameof(locks));
        Search = search ?? throw new ArgumentNullException(nameof(search));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _logger = logger ?? NullLogger<WebDavClient>.Instance;
    }

    /// <inheritdoc/>
    public IPropertyOperator Properties { get; }

    /// <inheritdoc/>
    public IResourceOperator Resources { get; }

    /// <inheritdoc/>
    public IFileOperator Files { get; }

    /// <inheritdoc/>
    public ILockOperator Locks { get; }

    /// <inheritdoc/>
    public ISearchOperator Search { get; }

    /// <summary>
    /// 使用默认配置创建 WebDAV 客户端.
    /// </summary>
    /// <param name="baseAddress">基础地址.</param>
    /// <returns>WebDAV 客户端实例.</returns>
    public static WebDavClient Create(Uri baseAddress)
    {
        return new WebDavClient(new WebDavClientOptions { BaseAddress = baseAddress });
    }

    /// <summary>
    /// 使用用户名和密码创建 WebDAV 客户端.
    /// </summary>
    /// <param name="baseAddress">基础地址.</param>
    /// <param name="userName">用户名.</param>
    /// <param name="password">密码.</param>
    /// <returns>WebDAV 客户端实例.</returns>
    public static WebDavClient Create(Uri baseAddress, string userName, string password)
    {
        return new WebDavClient(new WebDavClientOptions
        {
            BaseAddress = baseAddress,
            Credentials = new NetworkCredential(userName, password),
            UseDefaultCredentials = false,
        });
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _logger.LogDebug("Disposing WebDavClient");
        _dispatcher.Dispose();
        _disposed = true;
    }

    private static HttpClient CreateHttpClient(WebDavClientOptions options)
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            PreAuthenticate = options.PreAuthenticate,
            UseDefaultCredentials = options.UseDefaultCredentials,
            UseProxy = options.UseProxy,
        };

        if (options.Credentials != null)
        {
            handler.Credentials = options.Credentials;
            handler.UseDefaultCredentials = false;
        }

        if (options.Proxy != null)
        {
            handler.Proxy = options.Proxy;
        }

        return new HttpClient(handler, disposeHandler: true);
    }

    private static void ConfigureHttpClient(HttpClient client, WebDavClientOptions options)
    {
        if (options.BaseAddress != null)
        {
            client.BaseAddress = options.BaseAddress;
        }

        if (options.Timeout.HasValue)
        {
            client.Timeout = options.Timeout.Value;
        }

        foreach (var header in options.DefaultHeaders)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
    }
}

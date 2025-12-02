// Copyright (c) Richasy. All rights reserved.

using System.Text.RegularExpressions;

namespace Richasy.RodelReader.Sources.ZLibrary;

/// <summary>
/// ZLibrary 客户端实现.
/// </summary>
public sealed partial class ZLibraryClient : IZLibraryClient
{
    private readonly ZLibDispatcher _dispatcher;
    private readonly IHtmlParser _parser;
    private readonly ILogger<ZLibraryClient> _logger;
    private readonly HttpClient _httpClient;
    private bool _disposed;
    private string _mirror;

    /// <summary>
    /// 初始化 <see cref="ZLibraryClient"/> 类的新实例.
    /// </summary>
    public ZLibraryClient()
        : this(new ZLibraryClientOptions())
    {
    }

    /// <summary>
    /// 初始化 <see cref="ZLibraryClient"/> 类的新实例.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    public ZLibraryClient(ZLibraryClientOptions options)
        : this(options, null)
    {
    }

    /// <summary>
    /// 初始化 <see cref="ZLibraryClient"/> 类的新实例.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="logger">日志器.</param>
    public ZLibraryClient(ZLibraryClientOptions options, ILogger<ZLibraryClient>? logger)
        : this(options, CreateHttpClient(options), logger)
    {
    }

    /// <summary>
    /// 初始化 <see cref="ZLibraryClient"/> 类的新实例.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="logger">日志器.</param>
    public ZLibraryClient(ZLibraryClientOptions options, HttpClient httpClient, ILogger<ZLibraryClient>? logger)
    {
        Guard.NotNull(options);
        Guard.NotNull(httpClient);

        Options = options;
        _httpClient = httpClient;
        _logger = logger ?? NullLogger<ZLibraryClient>.Instance;
        _mirror = options.GetEffectiveDomain();

        _logger.LogDebug("Initializing ZLibraryClient with mirror: {Mirror}", _mirror);

        // 配置 HttpClient
        ConfigureHttpClient(httpClient, options);

        // 创建分发器
        _dispatcher = new ZLibDispatcher(httpClient, options.MaxConcurrentRequests, _logger);

        // 创建解析器
        _parser = new ZLibHtmlParser(_logger);

        // 创建各操作模块
        Search = new SearchProvider(_dispatcher, _parser, () => _mirror, () => IsAuthenticated, _logger);
        Books = new BookDetailProvider(_dispatcher, _parser, () => _mirror, () => IsAuthenticated, _logger);
        Profile = new ProfileProvider(_dispatcher, _parser, () => _mirror, () => IsAuthenticated, _logger);
        Booklists = new BooklistProvider(_dispatcher, _parser, () => _mirror, () => IsAuthenticated, _logger);

        _logger.LogInformation("ZLibraryClient initialized successfully");
    }

    /// <inheritdoc/>
    public ISearchProvider Search { get; }

    /// <inheritdoc/>
    public IBookDetailProvider Books { get; }

    /// <inheritdoc/>
    public IProfileProvider Profile { get; }

    /// <inheritdoc/>
    public IBooklistProvider Booklists { get; }

    /// <inheritdoc/>
    public ZLibraryClientOptions Options { get; }

    /// <inheritdoc/>
    public bool IsAuthenticated => _dispatcher.Cookies != null && _dispatcher.Cookies.Count > 0;

    /// <inheritdoc/>
    public string Mirror => _mirror;

    /// <inheritdoc/>
    public async Task LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(email);
        Guard.NotNullOrWhiteSpace(password);

        _logger.LogDebug("Logging in with email: {Email}", email);

        var formData = new Dictionary<string, string>
        {
            ["isModal"] = "true",
            ["email"] = email,
            ["password"] = password,
            ["site_mode"] = "books",
            ["action"] = "login",
            ["isSingleLogin"] = "1",
            ["redirectUrl"] = string.Empty,
            ["gg_json_mode"] = "1",
        };

        var loginUrl = Options.GetLoginUrl();

        string content;
        IEnumerable<string> setCookies;

        try
        {
            (content, setCookies) = await _dispatcher.PostAsync(
                loginUrl,
                formData,
                cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Login HTTP request failed");
            throw new LoginFailedException($"Login failed: {ex.Message}", ex);
        }

        var response = JsonSerializer.Deserialize(content, ZLibraryJsonContext.Default.LoginResponse);

        if (response?.Response?.ValidationError != null)
        {
            _logger.LogWarning("Login failed: validation error");
            throw new LoginFailedException($"Login failed: {content}");
        }

        // 解析 cookies
        var cookies = new Dictionary<string, string>();
        foreach (var cookie in setCookies)
        {
            var match = CookieRegex().Match(cookie);
            if (match.Success)
            {
                cookies[match.Groups[1].Value] = match.Groups[2].Value;
            }
        }

        _dispatcher.Cookies = cookies;

        _logger.LogInformation("Login successful, cookies set: {Count}", cookies.Count);
    }

    /// <inheritdoc/>
    public void Logout()
    {
        _dispatcher.Cookies = null;
        _logger.LogInformation("Logged out");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _httpClient.Dispose();
        _disposed = true;

        _logger.LogDebug("ZLibraryClient disposed");
    }

    private static HttpClient CreateHttpClient(ZLibraryClientOptions options)
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseCookies = false, // 我们手动管理 cookies
        };

        return new HttpClient(handler)
        {
            Timeout = options.Timeout,
        };
    }

    private static void ConfigureHttpClient(HttpClient httpClient, ZLibraryClientOptions options)
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
        httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
        httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
        httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
    }

    [GeneratedRegex(@"^([^=]+)=([^;]+)")]
    private static partial Regex CookieRegex();
}

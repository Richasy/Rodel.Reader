// Copyright (c) Richasy. All rights reserved.

using System.Text.RegularExpressions;

namespace Richasy.RodelReader.Sources.ZLibrary;

/// <summary>
/// ZLibrary 客户端实现.
/// </summary>
public sealed partial class ZLibraryClient : IZLibraryClient
{
    private const string SearchEndpoint = "/eapi/book/search";
    private const string ProfileEndpoint = "/eapi/user/profile";
    private const string DownloadLinkEndpoint = "/eapi/book/{0}/{1}/file";

    private static readonly Regex InvalidFileNameCharsRegex = new(@"[\\/:*?""<>|]", RegexOptions.Compiled);

    private readonly ZLibDispatcher _dispatcher;
    private readonly ILogger<ZLibraryClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _mirror;
    private bool _disposed;
    
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
        _dispatcher = new ZLibDispatcher(httpClient, options.MaxConcurrentRequests, _logger, options.CustomHeaders, _mirror);

        // 如果有初始 Cookies，设置它们
        if (options.InitialCookies != null && options.InitialCookies.Count > 0)
        {
            _dispatcher.Cookies = new Dictionary<string, string>(options.InitialCookies);
            _logger.LogDebug("Initial cookies set: {Count}", options.InitialCookies.Count);
        }

        _logger.LogInformation("ZLibraryClient initialized successfully");
    }

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
    public async Task<PagedResult<BookItem>> SearchAsync(
        string query,
        int page = 1,
        BookSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Guard.IsAuthenticated(IsAuthenticated);

        if (string.IsNullOrWhiteSpace(query))
        {
            throw new EmptyQueryException();
        }

        var url = $"{_mirror}{SearchEndpoint}";
        var formData = BuildSearchFormData(query, page, options);

        _logger.LogDebug("Searching books: {Query}, page: {Page}", query, page);

        var (json, _) = await _dispatcher.PostAsync(url, formData, cancellationToken).ConfigureAwait(false);
        var response = JsonSerializer.Deserialize(json, ZLibraryJsonContext.Default.SearchApiResponse);

        if (response?.Success != 1)
        {
            _logger.LogWarning("Search API returned unsuccessful response");
            return new PagedResult<BookItem>
            {
                Items = [],
                CurrentPage = page,
                TotalPages = 0,
                PageSize = options?.PageSize ?? 50,
            };
        }

        var books = ConvertToBookItems(response.Books);
        var pagination = response.Pagination;

        return new PagedResult<BookItem>
        {
            Items = books,
            CurrentPage = pagination?.Current ?? page,
            TotalPages = pagination?.TotalPages ?? 1,
            PageSize = pagination?.Limit ?? 50,
        };
    }

    /// <inheritdoc/>
    public async Task<UserProfile> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        Guard.IsAuthenticated(IsAuthenticated);

        var url = _mirror + ProfileEndpoint;

        _logger.LogDebug("Getting user profile");

        var json = await _dispatcher.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var response = JsonSerializer.Deserialize(json, ZLibraryJsonContext.Default.ProfileApiResponse);

        if (response?.Success != 1 || response.User == null)
        {
            throw new ParseException("Failed to parse user profile response");
        }

        var user = response.User;
        return new UserProfile
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            KindleEmail = user.KindleEmail,
            DownloadsToday = user.DownloadsToday,
            DownloadsLimit = user.DownloadsLimit,
            IsConfirmed = user.Confirmed == 1,
            IsPremium = user.IsPremium == 1,
        };
    }

    /// <inheritdoc/>
    public Task<DownloadInfo?> GetDownloadInfoAsync(BookItem book, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(book);

        if (string.IsNullOrEmpty(book.Id) || string.IsNullOrEmpty(book.Hash))
        {
            throw new ArgumentException("Book must have valid Id and Hash properties.");
        }

        return GetDownloadInfoAsync(book.Id, book.Hash, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DownloadInfo?> GetDownloadInfoAsync(string bookId, string bookHash, CancellationToken cancellationToken = default)
    {
        Guard.IsAuthenticated(IsAuthenticated);
        Guard.NotNullOrWhiteSpace(bookId);
        Guard.NotNullOrWhiteSpace(bookHash);

        var url = _mirror + string.Format(DownloadLinkEndpoint, bookId, bookHash);

        _logger.LogDebug("Getting download info for book: {BookId}, URL: {Url}", bookId, url);

        var json = await _dispatcher.GetAsync(url, cancellationToken).ConfigureAwait(false);
        
        _logger.LogDebug("Download API response: {Json}", json);
        
        var response = JsonSerializer.Deserialize(json, ZLibraryJsonContext.Default.DownloadApiResponse);

        if (response?.Success != 1 || response.File == null)
        {
            _logger.LogWarning("Download API returned unsuccessful response for book: {BookId}, Success: {Success}", bookId, response?.Success);
            return null;
        }

        if (!response.File.AllowDownload)
        {
            _logger.LogWarning("Download not allowed for book: {BookId}", bookId);
            return null;
        }

        if (string.IsNullOrEmpty(response.File.DownloadLink))
        {
            _logger.LogWarning("Download link is empty for book: {BookId}", bookId);
            return null;
        }

        // 清理文件名中的非法字符
        var fileName = response.File.Description ?? bookId;
        fileName = InvalidFileNameCharsRegex.Replace(fileName, string.Empty);

        return new DownloadInfo
        {
            DownloadLink = response.File.DownloadLink,
            FileName = fileName,
            Extension = response.File.Extension ?? string.Empty,
            Author = response.File.Author,
        };
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
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
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
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh,zh-CN;q=0.9,en-US;q=0.8,en;q=0.7");
        httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
    }

    private static Dictionary<string, string> BuildSearchFormData(string query, int page, BookSearchOptions? options)
    {
        var formData = new Dictionary<string, string>
        {
            ["message"] = query,
            ["page"] = page.ToString(),
        };

        if (options != null)
        {
            if (options.Exact)
            {
                formData["e"] = "1";
            }

            if (options.FromYear.HasValue)
            {
                formData["yearFrom"] = options.FromYear.Value.ToString();
            }

            if (options.ToYear.HasValue)
            {
                formData["yearTo"] = options.ToYear.Value.ToString();
            }

            if (options.Languages != null && options.Languages.Count > 0)
            {
                for (var i = 0; i < options.Languages.Count; i++)
                {
                    formData[$"languages[{i}]"] = options.Languages[i].ToString().ToLowerInvariant();
                }
            }

            if (options.Extensions != null && options.Extensions.Count > 0)
            {
                for (var i = 0; i < options.Extensions.Count; i++)
                {
                    formData[$"extensions[{i}]"] = options.Extensions[i].ToString().ToLowerInvariant();
                }
            }

            if (options.PageSize > 0)
            {
                formData["limit"] = options.PageSize.ToString();
            }
        }

        return formData;
    }

    private List<BookItem> ConvertToBookItems(IList<SearchApiBook>? apiBooks)
    {
        if (apiBooks == null || apiBooks.Count == 0)
        {
            return [];
        }

        var books = new List<BookItem>(apiBooks.Count);
        foreach (var apiBook in apiBooks)
        {
            var authors = !string.IsNullOrEmpty(apiBook.Author)
                ? apiBook.Author.Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList()
                : null;

            var bookUrl = !string.IsNullOrEmpty(apiBook.Href)
                ? $"{_mirror}{apiBook.Href}"
                : null;

            var downloadUrl = !string.IsNullOrEmpty(apiBook.Dl)
                ? $"{_mirror}{apiBook.Dl}"
                : null;

            books.Add(new BookItem
            {
                Id = apiBook.Id.ToString(),
                Name = apiBook.Title ?? "Unknown",
                Isbn = apiBook.Identifier,
                Url = bookUrl,
                CoverUrl = apiBook.Cover,
                Authors = authors,
                Publisher = apiBook.Publisher,
                Year = apiBook.Year > 0 ? apiBook.Year.ToString() : null,
                Language = apiBook.Language,
                Extension = apiBook.Extension,
                FileSize = apiBook.FilesizeString,
                Rating = apiBook.InterestScore,
                Quality = apiBook.QualityScore,
                DownloadUrl = downloadUrl,
                Description = apiBook.Description,
                Hash = apiBook.Hash,
            });
        }

        return books;
    }

    [GeneratedRegex(@"^([^=]+)=([^;]+)")]
    private static partial Regex CookieRegex();
}

// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.BookScraper.Internal;
using Richasy.RodelReader.Components.BookScraper.Internal.Responses;
using Richasy.RodelReader.Components.BookScraper.Internal.Responses.DouBan;

namespace Richasy.RodelReader.Components.BookScraper.Services;

/// <summary>
/// 豆瓣书籍刮削器.
/// </summary>
public sealed class DouBanBookScraper : IBookScraperFeature
{
    /// <summary>
    /// 刮削器唯一标识.
    /// </summary>
    public const string Id = "douban";

    private const string SearchUrl = "https://api.douban.com/v2/book/search";
    private const string BookUrl = "https://api.douban.com/v2/book/{0}";
    private const string WebUrl = "https://book.douban.com/subject/{0}";
    private const string ApiKey = "0ac44ae016490db2204ce0a042db2916";
    private const int DefaultCount = 20;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DouBanBookScraper>? _logger;

    /// <summary>
    /// 初始化 <see cref="DouBanBookScraper"/> 类的新实例.
    /// </summary>
    /// <param name="httpClientFactory">HTTP 客户端工厂.</param>
    /// <param name="logger">日志记录器.</param>
    public DouBanBookScraper(
        IHttpClientFactory httpClientFactory,
        ILogger<DouBanBookScraper>? logger = null)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public string FeatureId => Id;

    /// <inheritdoc/>
    public string FeatureName => "豆瓣读书";

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedCultures => ["zh-CN", "zh-TW"];

    /// <inheritdoc/>
    public string? IconUri => "https://img3.doubanio.com/favicon.ico";

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScrapedBook>> SearchBooksAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("豆瓣搜索: {Keyword}", keyword);

        var url = new Uri($"{SearchUrl}?q={Uri.EscapeDataString(keyword)}&count={DefaultCount}&apikey={ApiKey}");
        var client = _httpClientFactory.CreateClient(HttpClientNames.DouBan);

        try
        {
            var json = await client.GetStringAsync(url, cancellationToken).ConfigureAwait(false);
            var response = JsonSerializer.Deserialize(json, JsonContext.Default.DouBanSearchResponse);

            if (response?.Books is null || response.Books.Count == 0)
            {
                _logger?.LogInformation("豆瓣搜索无结果: {Keyword}", keyword);
                return Array.Empty<ScrapedBook>();
            }

            var result = response.Books
                .Where(b => !string.IsNullOrEmpty(b.Id) && !string.IsNullOrEmpty(b.Title))
                .Select(ConvertToScrapedBook)
                .ToList();

            _logger?.LogInformation("豆瓣搜索完成，找到 {Count} 本书籍", result.Count);
            return result.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "豆瓣搜索失败: {Keyword}", keyword);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ScrapedBook> GetBookDetailAsync(
        ScrapedBook book,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("获取豆瓣书籍详情: {BookId}", book.Id);

        var url = new Uri(string.Format(CultureInfo.InvariantCulture, BookUrl, book.Id) + $"?apikey={ApiKey}");
        var client = _httpClientFactory.CreateClient(HttpClientNames.DouBan);

        try
        {
            var json = await client.GetStringAsync(url, cancellationToken).ConfigureAwait(false);
            var doubanBook = JsonSerializer.Deserialize(json, JsonContext.Default.DouBanBook);

            if (doubanBook is null)
            {
                _logger?.LogWarning("豆瓣书籍详情解析失败: {BookId}", book.Id);
                return book;
            }

            var detailedBook = ConvertToScrapedBook(doubanBook);
            _logger?.LogInformation("获取豆瓣书籍详情成功: {Title}", detailedBook.Title);
            return detailedBook;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "获取豆瓣书籍详情失败: {BookId}", book.Id);
            throw;
        }
    }

    private ScrapedBook ConvertToScrapedBook(DouBanBook book)
    {
        var rating = RatingHelper.ParseAndNormalize(book.Rating?.Average, book.Rating?.Max ?? 10);
        var cover = book.Images?.Large ?? book.Images?.Medium ?? book.Image;
        var webLink = string.Format(CultureInfo.InvariantCulture, WebUrl, book.Id);

        return new ScrapedBook
        {
            Id = book.Id!,
            Title = book.Title!,
            ScraperId = FeatureId,
            Subtitle = book.Subtitle,
            Rating = rating,
            Description = book.Summary,
            Cover = UrlHelper.EnsureScheme(cover),
            WebLink = webLink,
            Author = book.Author is { Count: > 0 } ? string.Join(", ", book.Author) : null,
            Translator = book.Translator is { Count: > 0 } ? string.Join(", ", book.Translator) : null,
            Publisher = book.Publisher,
            PublishDate = book.PubDate,
            ISBN = book.Isbn13 ?? book.Isbn10,
            PageCount = int.TryParse(book.Pages, out var pages) ? pages : null,
        };
    }
}

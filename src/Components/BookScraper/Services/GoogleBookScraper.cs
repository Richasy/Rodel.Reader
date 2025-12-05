// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.BookScraper.Internal;
using Richasy.RodelReader.Components.BookScraper.Internal.Responses;
using Richasy.RodelReader.Components.BookScraper.Internal.Responses.Google;

namespace Richasy.RodelReader.Components.BookScraper.Services;

/// <summary>
/// Google Books 刮削器.
/// </summary>
public sealed class GoogleBookScraper : IBookScraper
{
    private const string SearchUrl = "https://www.googleapis.com/books/v1/volumes?q={0}";
    private const string DetailUrl = "https://www.googleapis.com/books/v1/volumes/{0}";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleBookScraper>? _logger;

    /// <summary>
    /// 初始化 <see cref="GoogleBookScraper"/> 类的新实例.
    /// </summary>
    /// <param name="httpClientFactory">HTTP 客户端工厂.</param>
    /// <param name="logger">日志记录器.</param>
    public GoogleBookScraper(
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleBookScraper>? logger = null)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public ScraperType Type => ScraperType.Google;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScrapedBook>> SearchBooksAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Google Books 搜索: {Keyword}", keyword);

        var url = string.Format(CultureInfo.InvariantCulture, SearchUrl, Uri.EscapeDataString(keyword));
        var client = _httpClientFactory.CreateClient(HttpClientNames.Scraper);

        try
        {
            var json = await client.GetStringAsync(new Uri(url), cancellationToken).ConfigureAwait(false);
            var response = JsonSerializer.Deserialize(json, JsonContext.Default.GoogleSearchResponse);

            if (response?.items is null || response.items.Length == 0)
            {
                _logger?.LogInformation("Google Books 搜索无结果: {Keyword}", keyword);
                return Array.Empty<ScrapedBook>();
            }

            var result = response.items
                .Where(p => p.volumeInfo is not null)
                .Select(ParseBook)
                .ToList();

            _logger?.LogInformation("Google Books 搜索完成，找到 {Count} 本书籍", result.Count);
            return result.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Google Books 搜索失败: {Keyword}", keyword);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ScrapedBook> GetBookDetailAsync(
        ScrapedBook book,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("获取 Google Books 书籍详情: {BookId}", book.Id);

        var url = string.Format(CultureInfo.InvariantCulture, DetailUrl, book.Id);
        var client = _httpClientFactory.CreateClient(HttpClientNames.Scraper);

        try
        {
            var json = await client.GetStringAsync(new Uri(url), cancellationToken).ConfigureAwait(false);
            var item = JsonSerializer.Deserialize(json, JsonContext.Default.GoogleBookItem);

            if (item is null)
            {
                _logger?.LogWarning("Google Books 书籍详情解析失败: {BookId}", book.Id);
                return book;
            }

            var detailedBook = ParseBook(item);
            _logger?.LogInformation("获取 Google Books 书籍详情成功: {Title}", detailedBook.Title);
            return detailedBook;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "获取 Google Books 书籍详情失败: {BookId}", book.Id);
            throw;
        }
    }

    private static ScrapedBook ParseBook(GoogleBookItem item)
    {
        var volumeInfo = item.volumeInfo;
        var author = volumeInfo.authors is not null
            ? string.Join(", ", volumeInfo.authors)
            : null;

        var description = volumeInfo.description ?? item.searchInfo?.textSnippet;
        var isbn = volumeInfo.industryIdentifiers?.FirstOrDefault()?.identifier;
        var category = volumeInfo.categories?.FirstOrDefault();

        return new ScrapedBook
        {
            Id = item.id,
            Title = volumeInfo.title,
            Subtitle = volumeInfo.subtitle ?? author,
            Cover = UrlHelper.EnsureScheme(volumeInfo.imageLinks?.thumbnail),
            Author = author,
            Description = description,
            WebLink = volumeInfo.infoLink,
            Publisher = volumeInfo.publisher ?? "--",
            PublishDate = volumeInfo.publishedDate,
            ISBN = isbn,
            Category = category,
            Source = ScraperType.Google,
        };
    }
}

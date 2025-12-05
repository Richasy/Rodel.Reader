// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.BookScraper.Internal;

namespace Richasy.RodelReader.Components.BookScraper.Services;

/// <summary>
/// 晋江文学城书籍刮削器.
/// </summary>
public sealed class JinJiangBookScraper : IBookScraper
{
    private const string SearchUrl = "https://www.jjwxc.net/search.php";
    private const string BookUrl = "https://www.jjwxc.net/onebook.php";

    private readonly IBrowsingContextFactory _browsingContextFactory;
    private readonly ILogger<JinJiangBookScraper>? _logger;

    /// <summary>
    /// 初始化 <see cref="JinJiangBookScraper"/> 类的新实例.
    /// </summary>
    /// <param name="browsingContextFactory">浏览上下文工厂.</param>
    /// <param name="logger">日志记录器.</param>
    public JinJiangBookScraper(
        IBrowsingContextFactory browsingContextFactory,
        ILogger<JinJiangBookScraper>? logger = null)
    {
        _browsingContextFactory = browsingContextFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public ScraperType Type => ScraperType.JinJiang;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScrapedBook>> SearchBooksAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("晋江文学城搜索: {Keyword}", keyword);

        // 晋江使用 GBK 编码
        var encodedKeyword = EncodingHelper.EncodeAsGbkUrl(keyword);
        var url = $"{SearchUrl}?kw={encodedKeyword}&t=1";
        var browsing = _browsingContextFactory.CreateContext();

        try
        {
            var document = await browsing.OpenAsync(url, cancellationToken).ConfigureAwait(false);
            var resultElements = document.QuerySelectorAll("#search_result div");
            var result = new List<ScrapedBook>();

            foreach (var element in resultElements)
            {
                var book = ParseSearchResult(element);
                if (book is not null)
                {
                    result.Add(book);
                }
            }

            _logger?.LogInformation("晋江文学城搜索完成，找到 {Count} 本书籍", result.Count);
            return result.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "晋江文学城搜索失败: {Keyword}", keyword);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ScrapedBook> GetBookDetailAsync(
        ScrapedBook book,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("获取晋江文学城书籍详情: {BookId}", book.Id);

        var url = $"{BookUrl}?novelid={book.Id}";
        var browsing = _browsingContextFactory.CreateContext();

        try
        {
            var document = await browsing.OpenAsync(url, cancellationToken).ConfigureAwait(false);
            var cover = document.QuerySelector(".noveldefaultimage")?.GetAttribute("src");

            var detailedBook = new ScrapedBook
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                PublishDate = book.PublishDate,
                WebLink = book.WebLink ?? url,
                Cover = UrlHelper.EnsureScheme(cover) ?? book.Cover,
                Rating = 3, // 晋江没有评分系统，使用默认值
                Publisher = "晋江文学城",
                Source = ScraperType.JinJiang,
            };

            _logger?.LogInformation("获取晋江文学城书籍详情成功: {Title}", detailedBook.Title);
            return detailedBook;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "获取晋江文学城书籍详情失败: {BookId}", book.Id);
            throw;
        }
    }

    private static ScrapedBook? ParseSearchResult(IElement element)
    {
        var titleElement = element.QuerySelector("h3.title");
        if (titleElement is null)
        {
            return null;
        }

        var titleLink = titleElement.QuerySelector("a");
        if (titleLink is null)
        {
            return null;
        }

        var title = titleLink.TextContent?.Trim();
        var href = titleLink.GetAttribute("href");

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(href))
        {
            return null;
        }

        if (!href.Contains("novelid", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // 从 URL 中提取 novelid
        var bookId = UrlHelper.ExtractQueryParam(href, "novelid");
        if (string.IsNullOrEmpty(bookId))
        {
            return null;
        }

        var publishDate = titleElement.QuerySelector("font")?.TextContent?.Trim()?.Trim('(', ')');
        var introduction = element.QuerySelector(".intro")?.TextContent?.Trim();
        var author = element.QuerySelector(".info a")?.TextContent?.Trim();

        return new ScrapedBook
        {
            Id = bookId,
            Title = title,
            Author = author,
            Description = introduction,
            PublishDate = publishDate,
            WebLink = href,
            Publisher = "晋江文学城",
            Source = ScraperType.JinJiang,
        };
    }
}

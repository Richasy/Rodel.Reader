// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.BookScraper.Internal;

namespace Richasy.RodelReader.Components.BookScraper.Services;

/// <summary>
/// Bangumi 书籍刮削器.
/// </summary>
public sealed class BangumiBookScraper : IBookScraper
{
    private const string SearchUrl = "https://bangumi.tv/subject_search/{0}?cat=1";
    private const string BookUrl = "https://bangumi.tv/subject/{0}";

    private readonly IBrowsingContextFactory _browsingContextFactory;
    private readonly ILogger<BangumiBookScraper>? _logger;

    /// <summary>
    /// 初始化 <see cref="BangumiBookScraper"/> 类的新实例.
    /// </summary>
    /// <param name="browsingContextFactory">浏览上下文工厂.</param>
    /// <param name="logger">日志记录器.</param>
    public BangumiBookScraper(
        IBrowsingContextFactory browsingContextFactory,
        ILogger<BangumiBookScraper>? logger = null)
    {
        _browsingContextFactory = browsingContextFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public ScraperType Type => ScraperType.Bangumi;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScrapedBook>> SearchBooksAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Bangumi 搜索: {Keyword}", keyword);

        var url = string.Format(CultureInfo.InvariantCulture, SearchUrl, Uri.EscapeDataString(keyword));
        var browsing = _browsingContextFactory.CreateContext();

        try
        {
            var document = await browsing.OpenAsync(url, cancellationToken).ConfigureAwait(false);
            var resultList = document.QuerySelectorAll("#browserItemList li");
            var result = new List<ScrapedBook>();

            foreach (var element in resultList)
            {
                var book = ParseSearchResult(element);
                if (book is not null)
                {
                    result.Add(book);
                }
            }

            _logger?.LogInformation("Bangumi 搜索完成，找到 {Count} 本书籍", result.Count);
            return result.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Bangumi 搜索失败: {Keyword}", keyword);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ScrapedBook> GetBookDetailAsync(
        ScrapedBook book,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("获取 Bangumi 书籍详情: {BookId}", book.Id);

        var url = string.Format(CultureInfo.InvariantCulture, BookUrl, book.Id);
        var browsing = _browsingContextFactory.CreateContext();

        try
        {
            var document = await browsing.OpenAsync(url, cancellationToken).ConfigureAwait(false);
            var info = document.QuerySelector("#bangumiInfo");

            if (info is null)
            {
                _logger?.LogWarning("Bangumi 书籍详情页面解析失败: {BookId}", book.Id);
                return book;
            }

            var cover = info.QuerySelector("a.cover img")?.GetAttribute("src");
            var summary = document.QuerySelector("#subject_summary")?.TextContent?.Trim();
            var ratingEle = document.QuerySelector(".global_score .number");
            var rating = book.Rating;

            if (ratingEle is not null)
            {
                rating = RatingHelper.ParseAndNormalize(ratingEle.TextContent, 10);
            }

            var detailedBook = new ScrapedBook
            {
                Id = book.Id,
                Title = book.Title,
                Rating = rating,
                Subtitle = book.Subtitle,
                Description = summary ?? book.Description,
                Cover = UrlHelper.EnsureScheme(cover) ?? book.Cover,
                WebLink = book.WebLink,
                Author = book.Author,
                Publisher = book.Publisher,
                PublishDate = book.PublishDate,
                Source = ScraperType.Bangumi,
            };

            _logger?.LogInformation("获取 Bangumi 书籍详情成功: {Title}", detailedBook.Title);
            return detailedBook;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "获取 Bangumi 书籍详情失败: {BookId}", book.Id);
            throw;
        }
    }

    private static ScrapedBook? ParseSearchResult(IElement element)
    {
        var headerEle = element.QuerySelector("a");
        if (headerEle is null)
        {
            return null;
        }

        var href = headerEle.GetAttribute("href");
        var id = href?.Split('/').LastOrDefault();

        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        var cover = element.QuerySelector(".image img")?.GetAttribute("src");
        var innerEle = element.QuerySelector(".inner");
        var title = innerEle?.QuerySelector("h3 a.l")?.TextContent?.Trim();

        if (string.IsNullOrEmpty(title))
        {
            return null;
        }

        var subtitle = innerEle?.QuerySelector("h3 small")?.TextContent?.Trim();
        var infoText = innerEle?.QuerySelector(".info")?.TextContent ?? string.Empty;
        var infoArray = infoText.Split('/');

        var time = infoArray.Length > 0 ? infoArray[0].Trim() : null;
        var author = infoArray.Length > 1 ? infoArray[1].Trim() : null;
        var publisher = infoArray.Length > 2 ? infoArray[2].Trim() : null;

        var ratingEle = element.QuerySelector(".rateInfo .fade");
        var rating = RatingHelper.ParseAndNormalize(ratingEle?.TextContent, 10);

        return new ScrapedBook
        {
            Id = id,
            Title = title,
            Rating = rating,
            Subtitle = subtitle,
            Cover = UrlHelper.EnsureScheme(cover),
            Author = author,
            Publisher = publisher,
            PublishDate = time,
            WebLink = $"https://bangumi.tv/subject/{id}",
            Source = ScraperType.Bangumi,
        };
    }
}

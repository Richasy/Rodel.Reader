// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.FanQie.Abstractions;

namespace Richasy.RodelReader.Components.BookScraper.Services;

/// <summary>
/// 番茄小说刮削器.
/// </summary>
public sealed class FanQieBookScraper : IBookScraperFeature
{
    /// <summary>
    /// 刮削器唯一标识.
    /// </summary>
    public const string Id = "fanqie";

    private readonly IFanQieClient _fanQieClient;
    private readonly ILogger<FanQieBookScraper>? _logger;

    /// <summary>
    /// 初始化 <see cref="FanQieBookScraper"/> 类的新实例.
    /// </summary>
    /// <param name="fanQieClient">番茄小说客户端.</param>
    /// <param name="logger">日志记录器.</param>
    public FanQieBookScraper(
        IFanQieClient fanQieClient,
        ILogger<FanQieBookScraper>? logger = null)
    {
        _fanQieClient = fanQieClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public string FeatureId => Id;

    /// <inheritdoc/>
    public string FeatureName => "番茄小说";

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedCultures => ["zh-CN"];

    /// <inheritdoc/>
    public string? IconUri => "https://fanqienovel.com/favicon.ico";

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScrapedBook>> SearchBooksAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("番茄小说搜索: {Keyword}", keyword);

        try
        {
            var searchResult = await _fanQieClient.SearchBooksAsync(keyword, 0, cancellationToken).ConfigureAwait(false);

            if (searchResult.Items is null || searchResult.Items.Count == 0)
            {
                _logger?.LogInformation("番茄小说搜索无结果: {Keyword}", keyword);
                return Array.Empty<ScrapedBook>();
            }

            var result = searchResult.Items.Select(ParseBookItem).ToList();
            _logger?.LogInformation("番茄小说搜索完成，找到 {Count} 本书籍", result.Count);
            return result.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "番茄小说搜索失败: {Keyword}", keyword);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ScrapedBook> GetBookDetailAsync(
        ScrapedBook book,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("获取番茄小说书籍详情: {BookId}", book.Id);

        try
        {
            var detail = await _fanQieClient.GetBookDetailAsync(book.Id, cancellationToken).ConfigureAwait(false);

            if (detail is null)
            {
                _logger?.LogWarning("番茄小说书籍详情获取失败: {BookId}", book.Id);
                return book;
            }

            var detailedBook = ParseBookDetail(detail);
            _logger?.LogInformation("获取番茄小说书籍详情成功: {Title}", detailedBook.Title);
            return detailedBook;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "获取番茄小说书籍详情失败: {BookId}", book.Id);
            throw;
        }
    }

    private ScrapedBook ParseBookItem(Sources.FanQie.Models.BookItem item)
    {
        var rating = 0;
        if (!string.IsNullOrEmpty(item.Score) &&
            double.TryParse(item.Score, NumberStyles.Any, CultureInfo.InvariantCulture, out var score))
        {
            rating = (int)Math.Round(Math.Clamp(score / 10.0 * 5, 1, 5));
        }

        var status = item.CreationStatus switch
        {
            Sources.FanQie.Models.BookCreationStatus.Ongoing => "连载中",
            Sources.FanQie.Models.BookCreationStatus.Completed => "已完结",
            _ => null,
        };

        return new ScrapedBook
        {
            Id = item.BookId,
            Title = item.Title,
            ScraperId = FeatureId,
            Author = item.Author,
            Cover = item.CoverUrl,
            Description = item.Abstract,
            Category = item.Category,
            Subtitle = status,
            Rating = rating,
            Publisher = "番茄小说",
            WebLink = $"https://fanqienovel.com/page/{item.BookId}",
        };
    }

    private ScrapedBook ParseBookDetail(Sources.FanQie.Models.BookDetail detail)
    {
        var status = detail.CreationStatus switch
        {
            Sources.FanQie.Models.BookCreationStatus.Ongoing => "连载中",
            Sources.FanQie.Models.BookCreationStatus.Completed => "已完结",
            _ => null,
        };

        var publishDate = detail.LastUpdateTime?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        return new ScrapedBook
        {
            Id = detail.BookId,
            Title = detail.Title,
            ScraperId = FeatureId,
            Author = detail.Author,
            Cover = detail.CoverUrl,
            Description = detail.Abstract,
            Category = detail.Category,
            Subtitle = status,
            Rating = 4, // 番茄小说详情页没有评分信息，使用默认值
            Publisher = "番茄小说",
            PublishDate = publishDate,
            WebLink = $"https://fanqienovel.com/page/{detail.BookId}",
        };
    }
}

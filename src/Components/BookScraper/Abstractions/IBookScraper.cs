// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.BookScraper.Abstractions;

/// <summary>
/// 书籍刮削器接口.
/// </summary>
public interface IBookScraper
{
    /// <summary>
    /// 刮削器类型.
    /// </summary>
    ScraperType Type { get; }

    /// <summary>
    /// 搜索书籍.
    /// </summary>
    /// <param name="keyword">搜索关键词.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>搜索结果列表.</returns>
    Task<IReadOnlyList<ScrapedBook>> SearchBooksAsync(
        string keyword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取书籍详情.
    /// </summary>
    /// <param name="book">基础书籍信息（通常来自搜索结果）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>包含详细信息的书籍.</returns>
    Task<ScrapedBook> GetBookDetailAsync(
        ScrapedBook book,
        CancellationToken cancellationToken = default);
}

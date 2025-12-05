// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Abstractions.Scrapers;

/// <summary>
/// 书籍刮削器功能接口.
/// 实现此接口以提供从在线服务获取书籍元数据的能力.
/// </summary>
public interface IBookScraperFeature : IPluginFeature
{
    /// <summary>
    /// 刮削器支持的语言/区域列表.
    /// 空列表表示支持所有语言.
    /// </summary>
    IReadOnlyList<string> SupportedCultures { get; }

    /// <summary>
    /// 刮削器图标 URL 或 Base64 编码的图标数据.
    /// </summary>
    string? IconUri { get; }

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

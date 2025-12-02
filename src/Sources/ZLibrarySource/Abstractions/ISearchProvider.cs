// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary;

/// <summary>
/// 搜索提供器接口.
/// </summary>
public interface ISearchProvider
{
    /// <summary>
    /// 搜索书籍.
    /// </summary>
    /// <param name="query">搜索关键词.</param>
    /// <param name="page">页码（从 1 开始）.</param>
    /// <param name="options">搜索选项.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>分页搜索结果.</returns>
    Task<PagedResult<BookItem>> SearchAsync(
        string query,
        int page = 1,
        BookSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 全文搜索书籍.
    /// </summary>
    /// <param name="query">搜索关键词.</param>
    /// <param name="page">页码（从 1 开始）.</param>
    /// <param name="options">搜索选项.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>分页搜索结果.</returns>
    Task<PagedResult<BookItem>> FullTextSearchAsync(
        string query,
        int page = 1,
        FullTextSearchOptions? options = null,
        CancellationToken cancellationToken = default);
}

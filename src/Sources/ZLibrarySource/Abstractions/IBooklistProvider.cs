// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary;

/// <summary>
/// 书单提供器接口.
/// </summary>
public interface IBooklistProvider
{
    /// <summary>
    /// 搜索公开书单.
    /// </summary>
    /// <param name="query">搜索关键词.</param>
    /// <param name="page">页码（从 1 开始）.</param>
    /// <param name="pageSize">每页数量.</param>
    /// <param name="order">排序方式.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>书单分页结果.</returns>
    Task<PagedResult<Booklist>> SearchPublicAsync(
        string query,
        int page = 1,
        int pageSize = 10,
        SortOrder order = SortOrder.Popular,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 搜索私有书单.
    /// </summary>
    /// <param name="query">搜索关键词.</param>
    /// <param name="page">页码（从 1 开始）.</param>
    /// <param name="pageSize">每页数量.</param>
    /// <param name="order">排序方式.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>书单分页结果.</returns>
    Task<PagedResult<Booklist>> SearchPrivateAsync(
        string query,
        int page = 1,
        int pageSize = 10,
        SortOrder order = SortOrder.Popular,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取书单中的书籍.
    /// </summary>
    /// <param name="booklistId">书单 ID.</param>
    /// <param name="page">页码（从 1 开始）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>书籍分页结果.</returns>
    Task<PagedResult<BookItem>> GetBooksInListAsync(
        string booklistId,
        int page = 1,
        CancellationToken cancellationToken = default);
}

// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary;

/// <summary>
/// 书籍详情提供器接口.
/// </summary>
public interface IBookDetailProvider
{
    /// <summary>
    /// 根据书籍 ID 获取详情.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>书籍详情.</returns>
    Task<BookDetail> GetByIdAsync(string bookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 URL 获取书籍详情.
    /// </summary>
    /// <param name="url">书籍详情页 URL.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>书籍详情.</returns>
    Task<BookDetail> GetByUrlAsync(string url, CancellationToken cancellationToken = default);
}

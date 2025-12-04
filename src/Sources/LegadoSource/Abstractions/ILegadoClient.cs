// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado;

/// <summary>
/// Legado 客户端接口.
/// </summary>
public interface ILegadoClient : IDisposable
{
    /// <summary>
    /// 获取客户端配置.
    /// </summary>
    LegadoClientOptions Options { get; }

    #region 书架管理

    /// <summary>
    /// 获取书架上的所有书籍.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>书籍列表.</returns>
    Task<IReadOnlyList<Book>> GetBookshelfAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存书籍到书架.
    /// </summary>
    /// <param name="book">书籍信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task SaveBookAsync(Book book, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从书架删除书籍.
    /// </summary>
    /// <param name="book">要删除的书籍.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task DeleteBookAsync(Book book, CancellationToken cancellationToken = default);

    #endregion

    #region 章节操作

    /// <summary>
    /// 获取书籍的章节列表.
    /// </summary>
    /// <param name="bookUrl">书籍链接.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>章节列表.</returns>
    Task<IReadOnlyList<Chapter>> GetChapterListAsync(string bookUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取章节内容.
    /// </summary>
    /// <param name="bookUrl">书籍链接.</param>
    /// <param name="chapterIndex">章节索引.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>章节内容.</returns>
    Task<ChapterContent> GetChapterContentAsync(string bookUrl, int chapterIndex, CancellationToken cancellationToken = default);

    #endregion

    #region 进度同步

    /// <summary>
    /// 保存阅读进度.
    /// </summary>
    /// <param name="progress">阅读进度.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task SaveProgressAsync(BookProgress progress, CancellationToken cancellationToken = default);

    #endregion

    #region 书源管理

    /// <summary>
    /// 获取所有书源.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>书源列表.</returns>
    Task<IReadOnlyList<BookSource>> GetBookSourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取单个书源.
    /// </summary>
    /// <param name="sourceUrl">书源链接.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>书源信息，不存在返回 null.</returns>
    Task<BookSource?> GetBookSourceAsync(string sourceUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存书源.
    /// </summary>
    /// <param name="source">书源信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task SaveBookSourceAsync(BookSource source, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存多个书源.
    /// </summary>
    /// <param name="sources">书源列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task SaveBookSourcesAsync(IEnumerable<BookSource> sources, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除多个书源.
    /// </summary>
    /// <param name="sources">要删除的书源列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task DeleteBookSourcesAsync(IEnumerable<BookSource> sources, CancellationToken cancellationToken = default);

    #endregion

    #region 封面

    /// <summary>
    /// 获取封面图片.
    /// </summary>
    /// <param name="coverPath">封面路径.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>封面图片流.</returns>
    Task<Stream> GetCoverAsync(string coverPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取完整的封面 URI.
    /// </summary>
    /// <param name="coverPath">封面路径.</param>
    /// <returns>完整的封面 URI.</returns>
    Uri GetCoverUri(string coverPath);

    #endregion
}

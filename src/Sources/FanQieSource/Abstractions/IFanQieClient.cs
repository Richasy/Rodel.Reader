// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Abstractions;

/// <summary>
/// 番茄小说客户端接口.
/// </summary>
public interface IFanQieClient : IDisposable
{
    /// <summary>
    /// 搜索书籍.
    /// </summary>
    /// <param name="query">搜索关键词.</param>
    /// <param name="offset">偏移量（默认0）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>搜索结果.</returns>
    Task<SearchResult<BookItem>> SearchBooksAsync(
        string query,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取书籍详情.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>书籍详情.</returns>
    Task<BookDetail?> GetBookDetailAsync(
        string bookId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取书籍目录（按卷分组）.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>卷列表.</returns>
    Task<IReadOnlyList<BookVolume>> GetBookTocAsync(
        string bookId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取单章内容.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="bookTitle">书籍标题.</param>
    /// <param name="chapter">章节信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>章节内容.</returns>
    Task<ChapterContent?> GetChapterContentAsync(
        string bookId,
        string bookTitle,
        ChapterItem chapter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量获取章节内容.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="bookTitle">书籍标题.</param>
    /// <param name="chapters">章节信息列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>章节内容列表.</returns>
    Task<IReadOnlyList<ChapterContent>> GetChapterContentsAsync(
        string bookId,
        string bookTitle,
        IEnumerable<ChapterItem> chapters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载整本书的所有章节内容（仅免费章节）.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="progress">进度回调（当前完成数, 总数）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>书籍详情及所有章节内容.</returns>
    Task<(BookDetail Detail, IReadOnlyList<ChapterContent> Chapters)> DownloadBookAsync(
        string bookId,
        IProgress<(int Current, int Total)>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载图片.
    /// </summary>
    /// <param name="imageUrl">图片 URL.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>图片二进制数据.</returns>
    Task<byte[]> DownloadImageAsync(
        string imageUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量下载图片.
    /// </summary>
    /// <param name="imageUrls">图片 URL 列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>图片 URL 与二进制数据的字典.</returns>
    Task<IReadOnlyDictionary<string, byte[]>> DownloadImagesAsync(
        IEnumerable<string> imageUrls,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取章节段评数量.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="chapterId">章节 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>段落索引与评论数量的映射，键为段落索引（字符串），值为评论数量.</returns>
    Task<IReadOnlyDictionary<string, int>?> GetCommentCountAsync(
        string bookId,
        string chapterId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取章节段评列表.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="chapterId">章节 ID.</param>
    /// <param name="paragraphIndex">段落索引.</param>
    /// <param name="offset">分页偏移量.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>评论列表结果.</returns>
    Task<CommentListResult?> GetCommentsAsync(
        string bookId,
        string chapterId,
        int paragraphIndex,
        string? offset = null,
        CancellationToken cancellationToken = default);
}

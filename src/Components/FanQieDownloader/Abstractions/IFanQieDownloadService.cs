// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Abstractions;

/// <summary>
/// 番茄小说下载服务接口.
/// </summary>
public interface IFanQieDownloadService
{
    /// <summary>
    /// 同步书籍（完整流程：分析 → 下载 → 生成 EPUB）.
    /// </summary>
    /// <param name="bookId">番茄书籍 ID.</param>
    /// <param name="options">同步选项.</param>
    /// <param name="progress">进度回调.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>同步结果，包含生成的 EPUB 路径.</returns>
    Task<SyncResult> SyncBookAsync(
        string bookId,
        SyncOptions options,
        IProgress<SyncProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分析现有 EPUB，提取番茄标记信息.
    /// </summary>
    /// <param name="epubPath">EPUB 文件路径.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>番茄书籍信息，如果不是番茄书籍则返回 null.</returns>
    Task<FanQieBookInfo?> AnalyzeEpubAsync(
        string epubPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 清理指定书籍的临时缓存.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="tempDirectory">临时目录.</param>
    Task CleanupCacheAsync(string bookId, string tempDirectory);

    /// <summary>
    /// 检查是否存在可续传的缓存.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="tempDirectory">临时目录.</param>
    /// <returns>缓存状态信息.</returns>
    Task<CacheState?> GetCacheStateAsync(string bookId, string tempDirectory);
}

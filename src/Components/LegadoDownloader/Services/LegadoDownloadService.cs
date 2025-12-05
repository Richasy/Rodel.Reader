// Copyright (c) Richasy. All rights reserved.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Richasy.RodelReader.Components.Legado.Abstractions;
using Richasy.RodelReader.Components.Legado.Internal;

namespace Richasy.RodelReader.Components.Legado.Services;

/// <summary>
/// Legado 下载服务实现.
/// </summary>
public sealed partial class LegadoDownloadService : ILegadoDownloadService
{
    private readonly ILegadoClient _client;
    private readonly IEpubBuilder _epubBuilder;
    private readonly ILogger<LegadoDownloadService>? _logger;

    /// <summary>
    /// 初始化 <see cref="LegadoDownloadService"/> 类的新实例.
    /// </summary>
    /// <param name="client">Legado 客户端.</param>
    /// <param name="epubBuilder">EPUB 构建器.</param>
    /// <param name="logger">日志记录器（可选）.</param>
    public LegadoDownloadService(
        ILegadoClient client,
        IEpubBuilder epubBuilder,
        ILogger<LegadoDownloadService>? logger = null)
    {
        _client = client;
        _epubBuilder = epubBuilder;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SyncResult> SyncBookAsync(
        Book book,
        SyncOptions options,
        IProgress<SyncProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var stats = new SyncStatisticsBuilder();
        CacheManager? cacheManager = null;
        LegadoBookInfo? bookInfo = null;
        EpubBook? existingBook = null;

        _logger?.LogInformation("开始同步书籍: {Title} ({BookUrl})", book.Name, book.BookUrl);

        try
        {
            // 确保输出目录存在
            Directory.CreateDirectory(options.OutputDirectory);
            Directory.CreateDirectory(options.TempDirectory);

            // 1. 分析现有 EPUB
            progress?.Report(SyncProgress.Analyzing());
            LegadoBookInfo? existingInfo = null;

            if (!options.ForceRedownload && !string.IsNullOrEmpty(options.ExistingEpubPath) && File.Exists(options.ExistingEpubPath))
            {
                _logger?.LogInformation("分析现有 EPUB: {Path}", options.ExistingEpubPath);
                existingBook = await EpubReader.ReadAsync(options.ExistingEpubPath).ConfigureAwait(false);
                existingInfo = await LegadoEpubAnalyzer.ExtractInfoAsync(existingBook, cancellationToken).ConfigureAwait(false);

                if (existingInfo != null && existingInfo.BookUrl != book.BookUrl)
                {
                    _logger?.LogWarning("现有 EPUB 的书籍链接不匹配: {ExistingUrl} vs {RequestedUrl}", existingInfo.BookUrl, book.BookUrl);
                    existingInfo = null;
                    existingBook.Dispose();
                    existingBook = null;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            // 2. 获取章节目录
            progress?.Report(SyncProgress.FetchingToc());
            _logger?.LogInformation("获取章节目录: {BookUrl}", book.BookUrl);

            var chapters = await _client.GetChapterListAsync(book.BookUrl, cancellationToken).ConfigureAwait(false);
            if (chapters == null || chapters.Count == 0)
            {
                _logger?.LogError("无法获取章节目录");
                return SyncResult.CreateFailure($"无法获取书籍目录: {book.Name}");
            }

            var allChaptersRaw = chapters.OrderBy(c => c.Index).ToList();

            // 基于完整目录计算哈希
            var tocHash = TocHashCalculator.Calculate(allChaptersRaw);

            // 应用章节范围过滤
            var allChapters = allChaptersRaw
                .Where(c => (!options.StartChapterIndex.HasValue || c.Index >= options.StartChapterIndex.Value) &&
                            (!options.EndChapterIndex.HasValue || c.Index <= options.EndChapterIndex.Value))
                .ToList();

            if (options.StartChapterIndex.HasValue || options.EndChapterIndex.HasValue)
            {
                _logger?.LogInformation(
                    "章节范围过滤: {Start} - {End}, 共 {Count} 章节",
                    options.StartChapterIndex ?? 0,
                    options.EndChapterIndex ?? allChaptersRaw.Count - 1,
                    allChapters.Count);
            }

            // 分离卷标题和正常章节
            var volumeChapters = allChapters.Where(c => c.IsVolume).ToList();
            var contentChapters = allChapters.Where(c => !c.IsVolume).ToList();

            stats.TotalChapters = allChapters.Count;
            stats.VolumeChapters = volumeChapters.Count;

            var serverUrl = _client.Options.BaseUrl;
            bookInfo = LegadoBookInfo.FromBook(book, serverUrl) with
            {
                TocHash = tocHash,
                LastSyncTime = DateTimeOffset.Now,
            };

            _logger?.LogInformation("书籍目录: {Count} 章节（含 {VolumeCount} 个卷标题）, 哈希: {Hash}",
                allChapters.Count, volumeChapters.Count, tocHash);

            cancellationToken.ThrowIfCancellationRequested();

            // 3. 检查缓存
            progress?.Report(SyncProgress.CheckingCache());
            cacheManager = new CacheManager(options.TempDirectory, book.BookUrl);

            var cacheState = await cacheManager.GetStateAsync().ConfigureAwait(false);
            var usableCache = cacheState?.IsValid(tocHash) == true;

            if (cacheManager.Exists() && !usableCache)
            {
                _logger?.LogInformation("目录已变化，清理旧缓存");
                cacheManager.Cleanup();
            }

            if (!cacheManager.Exists())
            {
                await cacheManager.InitializeAsync(book.BookUrl, tocHash, book.Name, book.Origin, serverUrl).ConfigureAwait(false);
            }

            // 4. 确定需要下载的章节
            var successfulChapterIndexes = new HashSet<int>();
            var failedChapterIndexes = new HashSet<int>();

            // 从现有 EPUB 获取章节状态
            if (existingInfo != null)
            {
                foreach (var index in existingInfo.DownloadedChapterIndexes)
                {
                    successfulChapterIndexes.Add(index);
                }

                if (options.RetryFailedChapters)
                {
                    foreach (var index in existingInfo.FailedChapterIndexes)
                    {
                        failedChapterIndexes.Add(index);
                    }
                }
            }

            // 从缓存获取断点续传信息
            if (usableCache && cacheState != null)
            {
                stats.RestoredFromCache = cacheState.CachedChapterIndexes.Count;
                foreach (var index in cacheState.CachedChapterIndexes)
                {
                    if (!successfulChapterIndexes.Contains(index) && !failedChapterIndexes.Contains(index))
                    {
                        successfulChapterIndexes.Add(index);
                    }
                }
            }

            // 计算需要下载的章节（仅内容章节，不含卷标题）
            var chaptersToDownload = contentChapters
                .Where(c => options.ForceRedownload ||
                            !successfulChapterIndexes.Contains(c.Index) ||
                            failedChapterIndexes.Contains(c.Index))
                .ToList();

            // 计算复用数
            var reusedFromEpub = contentChapters.Count(c =>
                successfulChapterIndexes.Contains(c.Index) && !failedChapterIndexes.Contains(c.Index));
            stats.Reused = reusedFromEpub;

            _logger?.LogInformation(
                "需要下载: {ToDownload} 章节, 复用: {Reused} 章节",
                chaptersToDownload.Count,
                stats.Reused);

            cancellationToken.ThrowIfCancellationRequested();

            // 5. 创建图片下载通道
            var imageChannel = Channel.CreateUnbounded<ImageDownloadTask>();

            // 6. 启动图片下载任务
            var imageDownloadTask = DownloadImagesFromChannelAsync(
                cacheManager,
                imageChannel.Reader,
                stats,
                progress,
                cancellationToken);

            // 7. 下载章节
            if (chaptersToDownload.Count > 0)
            {
                await DownloadChaptersAsync(
                    book.BookUrl,
                    chaptersToDownload,
                    cacheManager,
                    stats,
                    imageChannel,
                    progress,
                    options.ContinueOnError,
                    options.MaxConcurrentDownloads,
                    cancellationToken).ConfigureAwait(false);
            }

            // 8. 处理卷标题（保存为缓存）
            foreach (var volume in volumeChapters)
            {
                var cachedVolume = new CachedChapter
                {
                    ChapterIndex = volume.Index,
                    ChapterUrl = volume.Url,
                    Title = volume.Title,
                    IsVolume = true,
                    Status = ChapterStatus.Volume,
                    HtmlContent = PlaceholderGenerator.GenerateVolumeContent(volume.Index, volume.Title),
                    DownloadTime = DateTimeOffset.Now,
                };
                await cacheManager.SaveChapterAsync(cachedVolume).ConfigureAwait(false);
            }

            // 9. 下载封面图片
            if (!string.IsNullOrEmpty(book.CoverUrl) && IsAbsoluteUrl(book.CoverUrl) && !cacheManager.ImageExists("cover"))
            {
                _logger?.LogDebug("准备下载封面: {Url}", book.CoverUrl);
                await imageChannel.Writer.WriteAsync(new ImageDownloadTask
                {
                    ImageId = "cover",
                    Url = book.CoverUrl,
                }, cancellationToken).ConfigureAwait(false);
            }

            // 10. 关闭图片通道
            imageChannel.Writer.Complete();

            // 11. 等待所有图片下载完成
            await imageDownloadTask.ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            // 12. 生成 EPUB
            var epubPath = await GenerateEpubAsync(
                book,
                allChapters,
                cacheManager,
                existingBook,
                existingInfo,
                tocHash,
                options,
                stats,
                progress,
                cancellationToken).ConfigureAwait(false);

            // 13. 清理缓存
            progress?.Report(SyncProgress.CleaningUp());
            cacheManager.Cleanup();
            existingBook?.Dispose();
            existingBook = null;

            // 14. 完成
            stopwatch.Stop();
            stats.Duration = stopwatch.Elapsed;

            var finalStats = stats.Build();
            bookInfo = bookInfo with
            {
                DownloadedChapterIndexes = allChapters
                    .Where(c => !stats.FailedChapterIndexes.Contains(c.Index))
                    .Select(c => c.Index)
                    .ToList(),
                FailedChapterIndexes = stats.FailedChapterIndexes.ToList(),
            };

            progress?.Report(SyncProgress.Completed($"同步完成，共 {finalStats.TotalChapters} 章节"));

            _logger?.LogInformation(
                "同步完成: 新下载 {New}, 复用 {Reused}, 失败 {Failed}, 耗时 {Duration}",
                finalStats.NewlyDownloaded,
                finalStats.Reused,
                finalStats.Failed,
                finalStats.Duration);

            return SyncResult.CreateSuccess(epubPath, bookInfo, finalStats);
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation("同步已取消");
            progress?.Report(SyncProgress.Failed("同步已取消"));
            existingBook?.Dispose();
            return SyncResult.CreateCancelled(bookInfo);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "同步失败: {Message}", ex.Message);
            progress?.Report(SyncProgress.Failed(ex.Message));
            existingBook?.Dispose();
            return SyncResult.CreateFailure(ex.Message, bookInfo);
        }
    }

    /// <inheritdoc/>
    public async Task<LegadoBookInfo?> AnalyzeEpubAsync(
        string epubPath,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("分析 EPUB: {Path}", epubPath);
        return await LegadoEpubAnalyzer.AnalyzeAsync(epubPath, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task CleanupCacheAsync(string bookUrl, string tempDirectory)
    {
        _logger?.LogDebug("清理缓存: {BookUrl}", bookUrl);
        var cacheManager = new CacheManager(tempDirectory, bookUrl);
        cacheManager.Cleanup();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<CacheState?> GetCacheStateAsync(string bookUrl, string tempDirectory)
    {
        var cacheManager = new CacheManager(tempDirectory, bookUrl);
        return await cacheManager.GetStateAsync().ConfigureAwait(false);
    }

    private async Task DownloadChaptersAsync(
        string bookUrl,
        List<Chapter> chapters,
        CacheManager cacheManager,
        SyncStatisticsBuilder stats,
        Channel<ImageDownloadTask> imageChannel,
        IProgress<SyncProgress>? progress,
        bool continueOnError,
        int maxConcurrentDownloads,
        CancellationToken cancellationToken)
    {
        if (chapters.Count == 0)
        {
            return;
        }

        _logger?.LogInformation("开始下载 {Count} 个章节", chapters.Count);

        // 计算需要下载的章节（跳过已缓存的）
        var orderedChapters = chapters.OrderBy(c => c.Index).ToList();
        var chaptersToDownload = new List<Chapter>();
        var skippedCount = 0;

        foreach (var chapter in orderedChapters)
        {
            var cached = await cacheManager.LoadChapterAsync(chapter.Index).ConfigureAwait(false);
            if (cached?.Status == ChapterStatus.Downloaded)
            {
                skippedCount++;
                continue;
            }

            chaptersToDownload.Add(chapter);
        }

        var progressTracker = new DownloadProgressTracker { Total = chapters.Count };
        progressTracker.AddSkipped(skippedCount);

        if (chaptersToDownload.Count == 0)
        {
            _logger?.LogInformation("所有章节已缓存，无需下载");
            return;
        }

        _logger?.LogInformation("跳过 {Skipped} 个已缓存章节, 需下载 {ToDownload} 个章节",
            skippedCount, chaptersToDownload.Count);

        // 并发下载章节
        var concurrency = Math.Clamp(maxConcurrentDownloads, 1, 50);
        _logger?.LogInformation("使用 {Concurrency} 个并发连接下载", concurrency);

        using var semaphore = new SemaphoreSlim(concurrency, concurrency);
        var progressLock = new object();

        var downloadTasks = chaptersToDownload.Select(async chapter =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await DownloadSingleChapterAsync(
                    bookUrl,
                    chapter,
                    cacheManager,
                    stats,
                    imageChannel,
                    progress,
                    progressTracker,
                    progressLock,
                    continueOnError,
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        await Task.WhenAll(downloadTasks).ConfigureAwait(false);

        _logger?.LogInformation("章节下载完成: {Completed}/{Total}, 失败 {Failed}",
            progressTracker.Completed, progressTracker.Total, progressTracker.Failed);
    }

    /// <summary>
    /// 下载单个章节.
    /// </summary>
    private async Task DownloadSingleChapterAsync(
        string bookUrl,
        Chapter chapter,
        CacheManager cacheManager,
        SyncStatisticsBuilder stats,
        Channel<ImageDownloadTask> imageChannel,
        IProgress<SyncProgress>? progress,
        DownloadProgressTracker progressTracker,
        object progressLock,
        bool continueOnError,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger?.LogDebug("下载章节: {Index} {Title}", chapter.Index, chapter.Title);

            var content = await _client.GetChapterContentAsync(bookUrl, chapter.Index, cancellationToken).ConfigureAwait(false);

            if (content == null || string.IsNullOrWhiteSpace(content.Content))
            {
                throw new InvalidOperationException("章节内容为空");
            }

            // 处理 HTML 内容和提取图片
            var (processedHtml, imageRefs) = ProcessChapterContent(content.Content, chapter.Index);

            var cachedChapter = new CachedChapter
            {
                ChapterIndex = chapter.Index,
                ChapterUrl = chapter.Url,
                Title = content.Title ?? chapter.Title,
                IsVolume = chapter.IsVolume,
                Status = ChapterStatus.Downloaded,
                HtmlContent = processedHtml,
                DownloadTime = DateTimeOffset.Now,
                Images = imageRefs,
            };

            await cacheManager.SaveChapterAsync(cachedChapter).ConfigureAwait(false);

            // 将图片下载任务加入队列
            if (imageRefs?.Count > 0)
            {
                foreach (var imgRef in imageRefs)
                {
                    await imageChannel.Writer.WriteAsync(new ImageDownloadTask
                    {
                        ImageId = imgRef.ImageId,
                        Url = imgRef.Url,
                    }, cancellationToken).ConfigureAwait(false);
                }
            }

            lock (progressLock)
            {
                progressTracker.IncrementCompleted();
                stats.IncrementNewlyDownloaded();
                ReportDownloadProgress(progress, progressTracker, chapter.Title);
            }

            _logger?.LogDebug("章节下载完成: {Index} {Title}", chapter.Index, chapter.Title);
        }
        catch (Exception ex) when (continueOnError)
        {
            _logger?.LogWarning(ex, "章节下载失败: {Index} {Title}", chapter.Index, chapter.Title);

            var failedChapter = new CachedChapter
            {
                ChapterIndex = chapter.Index,
                ChapterUrl = chapter.Url,
                Title = chapter.Title,
                IsVolume = chapter.IsVolume,
                Status = ChapterStatus.Failed,
                FailureReason = ex.Message,
                DownloadTime = DateTimeOffset.Now,
            };

            await cacheManager.SaveChapterAsync(failedChapter).ConfigureAwait(false);

            lock (progressLock)
            {
                progressTracker.IncrementFailed();
                stats.IncrementFailed();
                stats.FailedChapterIndexes.Add(chapter.Index);
                ReportDownloadProgress(progress, progressTracker, chapter.Title);
            }
        }
    }

    /// <summary>
    /// 处理章节内容，提取图片链接.
    /// </summary>
    private (string processedHtml, List<CachedImageRef>? imageRefs) ProcessChapterContent(string content, int chapterIndex)
    {
        var imageRefs = new List<CachedImageRef>();
        var imageIndex = 0;

        // 提取所有图片链接
        var processedHtml = ImageSrcRegex().Replace(content, match =>
        {
            var imgSrc = match.Groups[1].Value;

            // 仅处理完整 URL（http/https）
            if (!IsAbsoluteUrl(imgSrc))
            {
                _logger?.LogDebug("忽略相对路径图片: {Src}", imgSrc);
                return match.Value; // 保留原样
            }

            var imageId = $"img_{chapterIndex}_{imageIndex}";
            imageIndex++;

            imageRefs.Add(new CachedImageRef
            {
                ImageId = imageId,
                Url = imgSrc,
                MediaType = GuessImageMediaType(imgSrc),
            });

            // 替换为本地路径（EPUB 内部路径）
            return $"<img src=\"../Images/{imageId}{GetImageExtension(imgSrc)}\"";
        });

        return (processedHtml, imageRefs.Count > 0 ? imageRefs : null);
    }

    /// <summary>
    /// 从通道消费图片下载任务并下载图片.
    /// </summary>
    private async Task DownloadImagesFromChannelAsync(
        CacheManager cacheManager,
        ChannelReader<ImageDownloadTask> imageReader,
        SyncStatisticsBuilder stats,
        IProgress<SyncProgress>? progress,
        CancellationToken cancellationToken)
    {
        var downloadedCount = 0;

        try
        {
            await foreach (var imageTask in imageReader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                // 跳过已存在的图片
                if (cacheManager.ImageExists(imageTask.ImageId))
                {
                    _logger?.LogDebug("跳过已存在的图片: {ImageId}", imageTask.ImageId);
                    continue;
                }

                try
                {
                    _logger?.LogDebug("下载图片: {ImageId} {Url}", imageTask.ImageId, imageTask.Url);

                    // 获取封面需要使用客户端的方法
                    byte[] data;
                    if (imageTask.ImageId == "cover")
                    {
                        using var stream = await _client.GetCoverAsync(imageTask.Url, cancellationToken).ConfigureAwait(false);
                        if (stream == null)
                        {
                            _logger?.LogWarning("封面下载失败: {Url}", imageTask.Url);
                            continue;
                        }

                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                        data = ms.ToArray();
                    }
                    else
                    {
                        // 对于章节内图片，使用 HttpClient 直接下载
                        using var httpClient = new HttpClient();
                        data = await httpClient.GetByteArrayAsync(new Uri(imageTask.Url), cancellationToken).ConfigureAwait(false);
                    }

                    await cacheManager.SaveImageAsync(imageTask.ImageId, data).ConfigureAwait(false);
                    stats.IncrementImagesDownloaded();
                    downloadedCount++;

                    progress?.Report(SyncProgress.DownloadingImages(0, $"已下载 {downloadedCount} 张图片"));
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "下载图片失败: {ImageId} {Url}", imageTask.ImageId, imageTask.Url);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 取消时正常退出
        }

        if (downloadedCount > 0)
        {
            _logger?.LogInformation("图片下载完成: {Downloaded} 张", downloadedCount);
        }
    }

    private async Task<string> GenerateEpubAsync(
        Book book,
        List<Chapter> allChapters,
        CacheManager cacheManager,
        EpubBook? existingBook,
        LegadoBookInfo? existingInfo,
        string tocHash,
        SyncOptions options,
        SyncStatisticsBuilder stats,
        IProgress<SyncProgress>? progress,
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation("开始生成 EPUB");

        var chapterInfos = new List<ChapterInfo>();
        var cachedChapters = await cacheManager.LoadAllChaptersAsync().ConfigureAwait(false);
        var cachedDict = cachedChapters.ToDictionary(c => c.ChapterIndex);
        var existingIndexes = existingInfo?.DownloadedChapterIndexes.ToHashSet() ?? [];

        var processedCount = 0;
        var totalCount = allChapters.Count;

        foreach (var chapter in allChapters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string htmlContent;
            var images = new List<ChapterImageInfo>();

            // 从缓存获取
            if (cachedDict.TryGetValue(chapter.Index, out var cached))
            {
                if ((cached.Status == ChapterStatus.Downloaded || cached.Status == ChapterStatus.Volume) &&
                    !string.IsNullOrEmpty(cached.HtmlContent))
                {
                    htmlContent = PlaceholderGenerator.WrapChapterContent(chapter.Index, cached.HtmlContent);

                    // 加载图片数据
                    if (cached.Images?.Count > 0)
                    {
                        foreach (var imgRef in cached.Images)
                        {
                            var imgData = await cacheManager.LoadImageAsync(imgRef.ImageId).ConfigureAwait(false);
                            if (imgData != null)
                            {
                                images.Add(new ChapterImageInfo
                                {
                                    Id = imgRef.ImageId,
                                    Offset = 0,
                                    ImageData = imgData,
                                    MediaType = imgRef.MediaType,
                                });
                            }
                        }
                    }
                }
                else
                {
                    htmlContent = PlaceholderGenerator.GenerateFailedPlaceholder(
                        chapter.Index,
                        chapter.Title,
                        cached.FailureReason);
                }
            }
            // 从现有 EPUB 复用
            else if (existingBook != null && existingIndexes.Contains(chapter.Index))
            {
                var existingChapter = await LegadoEpubAnalyzer.ReadChapterContentAsync(existingBook, chapter.Index).ConfigureAwait(false);
                if (existingChapter != null && !string.IsNullOrEmpty(existingChapter.BodyContent))
                {
                    htmlContent = existingChapter.BodyContent;

                    if (existingChapter.Images != null)
                    {
                        foreach (var img in existingChapter.Images)
                        {
                            images.Add(new ChapterImageInfo
                            {
                                Id = img.Id,
                                Offset = 0,
                                ImageData = img.Data,
                                MediaType = img.MediaType,
                            });
                        }
                    }
                }
                else
                {
                    htmlContent = PlaceholderGenerator.GenerateFailedPlaceholder(
                        chapter.Index,
                        chapter.Title,
                        "无法从现有 EPUB 读取");
                }
            }
            // 失败章节
            else
            {
                htmlContent = PlaceholderGenerator.GenerateFailedPlaceholder(
                    chapter.Index,
                    chapter.Title);

                if (!stats.FailedChapterIndexes.Contains(chapter.Index))
                {
                    stats.FailedChapterIndexes.Add(chapter.Index);
                    stats.Failed++;
                }
            }

            chapterInfos.Add(new ChapterInfo
            {
                Index = chapter.Index,
                Title = chapter.Title,
                Content = htmlContent,
                IsHtml = true,
                Images = images.Count > 0 ? images : null,
            });

            processedCount++;
            progress?.Report(SyncProgress.GeneratingEpub(new GenerateProgressDetail
            {
                Step = $"处理章节: {chapter.Title}",
                ProcessedChapters = processedCount,
                TotalChapters = totalCount,
            }));
        }

        // 释放现有 EPUB
        existingBook?.Dispose();

        // 准备封面
        CoverInfo? cover = null;
        var coverData = await cacheManager.LoadImageAsync("cover").ConfigureAwait(false);
        if (coverData != null)
        {
            cover = CoverInfo.FromBytes(coverData, GuessImageMediaType(book.CoverUrl ?? "cover.jpg"));
        }

        // 计算书籍 URL 的哈希作为唯一标识符
        var bookUrlHash = ComputeShortHash(book.BookUrl);

        // 准备元数据
        var metadata = new EpubMetadata
        {
            Title = book.Name,
            Author = book.Author,
            Description = book.Intro,
            Language = "zh",
            Identifier = $"legado-{bookUrlHash}",
            Cover = cover,
            CustomMetadata =
            [
                CustomMetadata.Create("legado:book-url", book.BookUrl),
                CustomMetadata.Create("legado:book-source", book.Origin ?? string.Empty),
                CustomMetadata.Create("legado:server-url", _client.Options.BaseUrl),
                CustomMetadata.Create("legado:sync-time", DateTimeOffset.Now.ToString("O")),
                CustomMetadata.Create("legado:toc-hash", tocHash),
                CustomMetadata.Create("legado:chapter-count", allChapters.Count.ToString()),
                .. (!stats.FailedChapterIndexes.IsEmpty
                    ? [CustomMetadata.Create("legado:failed-chapters", string.Join(",", stats.FailedChapterIndexes))]
                    : Array.Empty<CustomMetadata>()),
            ],
        };

        // 生成 EPUB
        var safeFileName = SanitizeFileName(book.Name);
        var epubFileName = $"{safeFileName}.epub";
        var epubPath = Path.Combine(options.OutputDirectory, epubFileName);

        progress?.Report(SyncProgress.GeneratingEpub(new GenerateProgressDetail
        {
            Step = "打包 EPUB 文件",
            ProcessedChapters = totalCount,
            TotalChapters = totalCount,
        }));

        await _epubBuilder.BuildToFileAsync(
            metadata,
            chapterInfos,
            epubPath,
            options.EpubOptions,
            cancellationToken).ConfigureAwait(false);

        _logger?.LogInformation("EPUB 已生成: {Path}", epubPath);

        return epubPath;
    }

    private static void ReportDownloadProgress(
        IProgress<SyncProgress>? progress,
        DownloadProgressTracker tracker,
        string currentChapter)
    {
        progress?.Report(SyncProgress.DownloadingChapters(new DownloadProgressDetail
        {
            Completed = tracker.Completed,
            Total = tracker.Total,
            Failed = tracker.Failed,
            Skipped = tracker.Skipped,
            CurrentChapter = currentChapter,
        }));
    }

    private static bool IsAbsoluteUrl(string url)
        => url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
           url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    private static string GuessImageMediaType(string url)
    {
        var lower = url.ToLowerInvariant();
        if (lower.Contains(".png", StringComparison.Ordinal))
        {
            return "image/png";
        }

        if (lower.Contains(".gif", StringComparison.Ordinal))
        {
            return "image/gif";
        }

        if (lower.Contains(".webp", StringComparison.Ordinal))
        {
            return "image/webp";
        }

        return "image/jpeg";
    }

    private static string GetImageExtension(string url)
    {
        var lower = url.ToLowerInvariant();
        if (lower.Contains(".png", StringComparison.Ordinal))
        {
            return ".png";
        }

        if (lower.Contains(".gif", StringComparison.Ordinal))
        {
            return ".gif";
        }

        if (lower.Contains(".webp", StringComparison.Ordinal))
        {
            return ".webp";
        }

        return ".jpg";
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new StringBuilder(fileName);
        foreach (var c in invalidChars)
        {
            sanitized.Replace(c, '_');
        }

        return sanitized.ToString();
    }

    private static string ComputeShortHash(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }

    [GeneratedRegex(@"<img[^>]+src=[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex ImageSrcRegex();

    /// <summary>
    /// 同步统计构建器.
    /// </summary>
    private sealed class SyncStatisticsBuilder
    {
        private int _totalChapters;
        private int _newlyDownloaded;
        private int _reused;
        private int _failed;
        private int _restoredFromCache;
        private int _imagesDownloaded;
        private int _volumeChapters;

        public int TotalChapters { get => _totalChapters; set => _totalChapters = value; }
        public int NewlyDownloaded { get => _newlyDownloaded; set => _newlyDownloaded = value; }
        public int Reused { get => _reused; set => _reused = value; }
        public int Failed { get => _failed; set => _failed = value; }
        public int RestoredFromCache { get => _restoredFromCache; set => _restoredFromCache = value; }
        public int ImagesDownloaded { get => _imagesDownloaded; set => _imagesDownloaded = value; }
        public int VolumeChapters { get => _volumeChapters; set => _volumeChapters = value; }
        public TimeSpan Duration { get; set; }
        public ConcurrentBag<int> FailedChapterIndexes { get; } = [];

        public void IncrementNewlyDownloaded() => Interlocked.Increment(ref _newlyDownloaded);
        public void IncrementFailed() => Interlocked.Increment(ref _failed);
        public void IncrementImagesDownloaded() => Interlocked.Increment(ref _imagesDownloaded);

        public SyncStatistics Build() => new()
        {
            TotalChapters = TotalChapters,
            NewlyDownloaded = NewlyDownloaded,
            Reused = Reused,
            Failed = Failed,
            RestoredFromCache = RestoredFromCache,
            ImagesDownloaded = ImagesDownloaded,
            VolumeChapters = VolumeChapters,
            Duration = Duration,
        };
    }

    /// <summary>
    /// 图片下载任务.
    /// </summary>
    private sealed class ImageDownloadTask
    {
        public required string ImageId { get; init; }
        public required string Url { get; init; }
    }

    /// <summary>
    /// 下载进度跟踪器（线程安全）.
    /// </summary>
    private sealed class DownloadProgressTracker
    {
        private int _completed;
        private int _failed;
        private int _skipped;

        public int Completed => _completed;
        public int Failed => _failed;
        public int Skipped => _skipped;
        public int Total { get; init; }

        public void IncrementCompleted() => Interlocked.Increment(ref _completed);
        public void IncrementFailed() => Interlocked.Increment(ref _failed);
        public void IncrementSkipped() => Interlocked.Increment(ref _skipped);
        public void AddSkipped(int count) => Interlocked.Add(ref _skipped, count);
    }
}

// Copyright (c) Richasy. All rights reserved.

using System.Diagnostics;
using Richasy.RodelReader.Components.FanQie.Abstractions;
using Richasy.RodelReader.Components.FanQie.Internal;

namespace Richasy.RodelReader.Components.FanQie.Services;

/// <summary>
/// 番茄小说下载服务实现.
/// </summary>
public sealed class FanQieDownloadService : IFanQieDownloadService
{
    private readonly IFanQieClient _client;
    private readonly IEpubBuilder _epubBuilder;
    private readonly ILogger<FanQieDownloadService>? _logger;

    /// <summary>
    /// 初始化 <see cref="FanQieDownloadService"/> 类的新实例.
    /// </summary>
    /// <param name="client">番茄小说客户端.</param>
    /// <param name="epubBuilder">EPUB 构建器.</param>
    /// <param name="logger">日志记录器（可选）.</param>
    public FanQieDownloadService(
        IFanQieClient client,
        IEpubBuilder epubBuilder,
        ILogger<FanQieDownloadService>? logger = null)
    {
        _client = client;
        _epubBuilder = epubBuilder;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SyncResult> SyncBookAsync(
        string bookId,
        SyncOptions options,
        IProgress<SyncProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var stats = new SyncStatisticsBuilder();
        CacheManager? cacheManager = null;
        FanQieBookInfo? bookInfo = null;
        EpubBook? existingBook = null;

        try
        {
            // 确保输出目录存在
            Directory.CreateDirectory(options.OutputDirectory);
            Directory.CreateDirectory(options.TempDirectory);

            // 1. 分析现有 EPUB
            progress?.Report(SyncProgress.Analyzing());
            FanQieBookInfo? existingInfo = null;

            if (!options.ForceRedownload && !string.IsNullOrEmpty(options.ExistingEpubPath) && File.Exists(options.ExistingEpubPath))
            {
                _logger?.LogInformation("分析现有 EPUB: {Path}", options.ExistingEpubPath);
                existingBook = await EpubReader.ReadAsync(options.ExistingEpubPath).ConfigureAwait(false);
                existingInfo = await FanQieEpubAnalyzer.ExtractInfoAsync(existingBook, cancellationToken).ConfigureAwait(false);

                if (existingInfo != null && existingInfo.BookId != bookId)
                {
                    _logger?.LogWarning("现有 EPUB 的书籍 ID 不匹配: {ExistingId} vs {RequestedId}", existingInfo.BookId, bookId);
                    existingInfo = null;
                    existingBook.Dispose();
                    existingBook = null;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            // 2. 获取在线书籍信息和目录
            progress?.Report(SyncProgress.FetchingToc());
            _logger?.LogInformation("获取书籍详情: {BookId}", bookId);

            var bookDetail = await _client.GetBookDetailAsync(bookId, cancellationToken).ConfigureAwait(false);
            if (bookDetail == null)
            {
                return SyncResult.CreateFailure($"无法获取书籍信息: {bookId}");
            }

            var volumes = await _client.GetBookTocAsync(bookId, cancellationToken).ConfigureAwait(false);
            if (volumes == null || volumes.Count == 0)
            {
                return SyncResult.CreateFailure($"无法获取书籍目录: {bookId}");
            }

            var allChaptersRaw = volumes.SelectMany(v => v.Chapters).OrderBy(c => c.Order).ToList();

            // 基于完整目录计算哈希，这样章节范围变化不会导致缓存失效
            var tocHash = TocHashCalculator.Calculate(allChaptersRaw.Select(c => c.ItemId));

            // 应用章节范围过滤
            var allChapters = allChaptersRaw
                .Where(c => (!options.StartChapterOrder.HasValue || c.Order >= options.StartChapterOrder.Value) &&
                            (!options.EndChapterOrder.HasValue || c.Order <= options.EndChapterOrder.Value))
                .ToList();

            if (options.StartChapterOrder.HasValue || options.EndChapterOrder.HasValue)
            {
                _logger?.LogInformation(
                    "章节范围过滤: {Start} - {End}, 共 {Count} 章节",
                    options.StartChapterOrder ?? 1,
                    options.EndChapterOrder ?? allChaptersRaw.Count,
                    allChapters.Count);
            }

            stats.TotalChapters = allChapters.Count;

            bookInfo = FanQieBookInfo.FromBookDetail(bookDetail) with
            {
                TocHash = tocHash,
                LastSyncTime = DateTimeOffset.Now,
            };

            _logger?.LogInformation("书籍目录: {Count} 章节, 哈希: {Hash}", allChapters.Count, tocHash);

            cancellationToken.ThrowIfCancellationRequested();

            // 3. 检查缓存（缓存用于断点续传，不影响已完成的 EPUB）
            progress?.Report(SyncProgress.CheckingCache());
            cacheManager = new CacheManager(options.TempDirectory, bookId);

            // 缓存仅在同一次同步会话中有效，目录变化时清理
            var cacheState = await cacheManager.GetStateAsync().ConfigureAwait(false);
            var usableCache = cacheState?.IsValid(tocHash) == true;

            if (cacheManager.Exists() && !usableCache)
            {
                _logger?.LogInformation("目录已变化，清理旧缓存");
                cacheManager.Cleanup();
            }

            if (!cacheManager.Exists())
            {
                await cacheManager.InitializeAsync(tocHash, bookDetail.Title).ConfigureAwait(false);
            }

            // 4. 确定需要下载的章节
            // 优先级：1. 从现有 EPUB 判断章节状态  2. 从缓存获取断点续传信息
            var successfulChapterIds = new HashSet<string>();  // 已成功下载的章节
            var failedChapterIds = new HashSet<string>();      // 失败的章节（需要重试）

            // 从现有 EPUB 获取章节状态（这是最可靠的来源）
            if (existingInfo != null)
            {
                // 成功下载的章节
                foreach (var id in existingInfo.DownloadedChapterIds)
                {
                    successfulChapterIds.Add(id);
                }

                // 失败的章节（如果启用重试）
                if (options.RetryFailedChapters)
                {
                    foreach (var id in existingInfo.FailedChapterIds)
                    {
                        failedChapterIds.Add(id);
                    }
                }
            }

            // 从缓存获取断点续传信息（补充，不覆盖 EPUB 的判断）
            if (usableCache && cacheState != null)
            {
                stats.RestoredFromCache = cacheState.CachedChapterIds.Count;
                foreach (var id in cacheState.CachedChapterIds)
                {
                    // 只添加不在 EPUB 中的章节（断点续传的新下载）
                    if (!successfulChapterIds.Contains(id) && !failedChapterIds.Contains(id))
                    {
                        successfulChapterIds.Add(id);
                    }
                }
            }

            // 计算需要下载的章节：
            // - 新章节（不在成功列表中）
            // - 失败章节（在失败列表中，需要重试）
            // - 强制重下载时包含所有章节
            var chaptersToDownload = allChapters
                .Where(c => !c.IsLocked && !c.NeedPay)
                .Where(c => options.ForceRedownload ||
                            !successfulChapterIds.Contains(c.ItemId) ||
                            failedChapterIds.Contains(c.ItemId))
                .ToList();

            var lockedChapters = allChapters.Where(c => c.IsLocked || c.NeedPay).ToList();
            stats.LockedChapters = lockedChapters.Count;

            // 计算复用数：在当前范围内已成功下载且不需要重新下载的章节
            var reusedFromEpub = allChapters
                .Where(c => !c.IsLocked && !c.NeedPay)
                .Count(c => successfulChapterIds.Contains(c.ItemId) && !failedChapterIds.Contains(c.ItemId));
            stats.Reused = reusedFromEpub;

            _logger?.LogInformation(
                "需要下载: {ToDownload} 章节, 复用: {Reused} 章节, 锁定: {Locked} 章节",
                chaptersToDownload.Count,
                stats.Reused,
                stats.LockedChapters);

            cancellationToken.ThrowIfCancellationRequested();

            // 5. 下载章节
            if (chaptersToDownload.Count > 0)
            {
                await DownloadChaptersAsync(
                    bookId,
                    bookDetail.Title,
                    chaptersToDownload,
                    cacheManager,
                    stats,
                    progress,
                    options.ContinueOnError,
                    cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();

            // 6. 下载图片
            await DownloadImagesAsync(
                cacheManager,
                bookDetail.CoverUrl,
                stats,
                progress,
                cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            // 7. 生成 EPUB
            var epubPath = await GenerateEpubAsync(
                bookId,
                bookDetail,
                allChapters,
                lockedChapters,
                cacheManager,
                existingBook,
                existingInfo,
                tocHash,
                options,
                stats,
                progress,
                cancellationToken).ConfigureAwait(false);

            // 8. 清理缓存
            progress?.Report(SyncProgress.CleaningUp());
            cacheManager.Cleanup();
            existingBook?.Dispose();
            existingBook = null;

            // 9. 完成
            stopwatch.Stop();
            stats.Duration = stopwatch.Elapsed;

            var finalStats = stats.Build();
            bookInfo = bookInfo with
            {
                DownloadedChapterIds = allChapters
                    .Where(c => !lockedChapters.Contains(c))
                    .Where(c => !stats.FailedChapterIds.Contains(c.ItemId))
                    .Select(c => c.ItemId)
                    .ToList(),
                FailedChapterIds = stats.FailedChapterIds.ToList(),
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
            progress?.Report(SyncProgress.Cancelled());
            existingBook?.Dispose();
            // 保留缓存以便断点续传
            return SyncResult.CreateCancelled(bookInfo);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "同步失败");
            progress?.Report(SyncProgress.Failed(ex.Message));
            existingBook?.Dispose();
            // 保留缓存以便断点续传
            return SyncResult.CreateFailure(ex.Message, bookInfo);
        }
    }

    /// <inheritdoc/>
    public async Task<FanQieBookInfo?> AnalyzeEpubAsync(
        string epubPath,
        CancellationToken cancellationToken = default)
    {
        return await FanQieEpubAnalyzer.AnalyzeAsync(epubPath, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task CleanupCacheAsync(string bookId, string tempDirectory)
    {
        var cacheManager = new CacheManager(tempDirectory, bookId);
        cacheManager.Cleanup();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<CacheState?> GetCacheStateAsync(string bookId, string tempDirectory)
    {
        var cacheManager = new CacheManager(tempDirectory, bookId);
        return await cacheManager.GetStateAsync().ConfigureAwait(false);
    }

    private async Task DownloadChaptersAsync(
        string bookId,
        string bookTitle,
        List<ChapterItem> chapters,
        CacheManager cacheManager,
        SyncStatisticsBuilder stats,
        IProgress<SyncProgress>? progress,
        bool continueOnError,
        CancellationToken cancellationToken)
    {
        if (chapters.Count == 0)
        {
            return;
        }

        var completed = 0;
        var failed = 0;
        var skipped = 0;
        var total = chapters.Count;

        // 构建章节信息映射
        var chapterInfoMap = chapters.ToDictionary(c => c.ItemId, c => c);

        // 计算需要下载的范围（跳过已缓存的章节）
        var orderedChapters = chapters.OrderBy(c => c.Order).ToList();
        var chaptersToDownload = new List<ChapterItem>();

        foreach (var chapter in orderedChapters)
        {
            // 检查缓存
            var cached = await cacheManager.LoadChapterAsync(chapter.ItemId).ConfigureAwait(false);
            if (cached?.Status == ChapterStatus.Downloaded)
            {
                skipped++;
                ReportDownloadProgress(progress, completed, total, failed, skipped, chapter.Title);
                continue;
            }

            chaptersToDownload.Add(chapter);
        }

        if (chaptersToDownload.Count == 0)
        {
            return;
        }

        // 计算连续范围
        var ranges = CalculateDownloadRanges(chaptersToDownload);
        _logger?.LogInformation("需要下载 {Count} 个范围: {Ranges}", ranges.Count, string.Join(", ", ranges.Select(r => r.Range)));

        // 按范围批量下载
        foreach (var rangeInfo in ranges)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _logger?.LogDebug("下载范围: {Range}", rangeInfo.Range);

                var contents = await _client.GetChapterContentsByRangeAsync(
                    bookId,
                    bookTitle,
                    rangeInfo.Range,
                    chapterInfoMap,
                    cancellationToken).ConfigureAwait(false);

                // 处理下载结果
                foreach (var content in contents)
                {
                    if (!chapterInfoMap.TryGetValue(content.ItemId, out var chapterInfo))
                    {
                        continue;
                    }

                    // 添加段落标记
                    var markedHtml = ChapterContentMarker.AddMarkers(content.HtmlContent, content.ItemId);

                    var cachedChapter = new CachedChapter
                    {
                        ChapterId = content.ItemId,
                        Title = content.Title,
                        Order = content.Order,
                        VolumeName = content.VolumeName,
                        Status = ChapterStatus.Downloaded,
                        HtmlContent = markedHtml,
                        WordCount = content.WordCount,
                        DownloadTime = DateTimeOffset.Now,
                        Images = content.Images?.Select(img => new CachedImageRef
                        {
                            ImageId = img.Id,
                            Url = img.Url,
                            MediaType = GuessImageMediaType(img.Url),
                        }).ToList(),
                    };

                    await cacheManager.SaveChapterAsync(cachedChapter).ConfigureAwait(false);
                    completed++;
                    stats.NewlyDownloaded++;
                    ReportDownloadProgress(progress, completed, total, failed, skipped, content.Title);
                }

                // 检查是否有章节未返回（下载失败）
                var downloadedIds = contents.Select(c => c.ItemId).ToHashSet();
                foreach (var chapter in rangeInfo.Chapters)
                {
                    if (!downloadedIds.Contains(chapter.ItemId))
                    {
                        _logger?.LogWarning("章节未返回: {ChapterId} {Title}", chapter.ItemId, chapter.Title);

                        if (continueOnError)
                        {
                            var failedChapter = new CachedChapter
                            {
                                ChapterId = chapter.ItemId,
                                Title = chapter.Title,
                                Order = chapter.Order,
                                VolumeName = chapter.VolumeName,
                                Status = ChapterStatus.Failed,
                                FailureReason = "服务器未返回此章节",
                                DownloadTime = DateTimeOffset.Now,
                            };

                            await cacheManager.SaveChapterAsync(failedChapter).ConfigureAwait(false);
                            failed++;
                            stats.Failed++;
                            stats.FailedChapterIds.Add(chapter.ItemId);
                            ReportDownloadProgress(progress, completed, total, failed, skipped, chapter.Title);
                        }
                    }
                }
            }
            catch (Exception ex) when (continueOnError)
            {
                _logger?.LogWarning(ex, "下载范围失败: {Range}", rangeInfo.Range);

                // 将范围内所有章节标记为失败
                foreach (var chapter in rangeInfo.Chapters)
                {
                    var failedChapter = new CachedChapter
                    {
                        ChapterId = chapter.ItemId,
                        Title = chapter.Title,
                        Order = chapter.Order,
                        VolumeName = chapter.VolumeName,
                        Status = ChapterStatus.Failed,
                        FailureReason = ex.Message,
                        DownloadTime = DateTimeOffset.Now,
                    };

                    await cacheManager.SaveChapterAsync(failedChapter).ConfigureAwait(false);
                    failed++;
                    stats.Failed++;
                    stats.FailedChapterIds.Add(chapter.ItemId);
                    ReportDownloadProgress(progress, completed, total, failed, skipped, chapter.Title);
                }
            }
        }
    }

    /// <summary>
    /// 计算需要下载的连续范围.
    /// </summary>
    /// <param name="chapters">需要下载的章节列表（已按 Order 排序）.</param>
    /// <returns>范围列表.</returns>
    private static List<DownloadRangeInfo> CalculateDownloadRanges(List<ChapterItem> chapters)
    {
        if (chapters.Count == 0)
        {
            return [];
        }

        var ranges = new List<DownloadRangeInfo>();
        var currentRangeChapters = new List<ChapterItem> { chapters[0] };
        var startOrder = chapters[0].Order;
        var endOrder = chapters[0].Order;

        for (var i = 1; i < chapters.Count; i++)
        {
            if (chapters[i].Order == endOrder + 1)
            {
                // 连续
                endOrder = chapters[i].Order;
                currentRangeChapters.Add(chapters[i]);
            }
            else
            {
                // 断开，保存当前范围
                ranges.Add(new DownloadRangeInfo
                {
                    Range = $"{startOrder}-{endOrder}",
                    Chapters = currentRangeChapters,
                });

                // 开始新范围
                startOrder = chapters[i].Order;
                endOrder = chapters[i].Order;
                currentRangeChapters = [chapters[i]];
            }
        }

        // 保存最后一个范围
        ranges.Add(new DownloadRangeInfo
        {
            Range = $"{startOrder}-{endOrder}",
            Chapters = currentRangeChapters,
        });

        return ranges;
    }

    /// <summary>
    /// 下载范围信息.
    /// </summary>
    private sealed class DownloadRangeInfo
    {
        public required string Range { get; init; }
        public required List<ChapterItem> Chapters { get; init; }
    }

    private async Task DownloadImagesAsync(
        CacheManager cacheManager,
        string? coverUrl,
        SyncStatisticsBuilder stats,
        IProgress<SyncProgress>? progress,
        CancellationToken cancellationToken)
    {
        // 收集所有需要下载的图片
        var imageUrls = new Dictionary<string, string>(); // imageId -> url

        // 封面
        if (!string.IsNullOrEmpty(coverUrl) && !cacheManager.ImageExists("cover"))
        {
            imageUrls["cover"] = coverUrl;
        }

        // 章节图片
        var chapters = await cacheManager.LoadAllChaptersAsync().ConfigureAwait(false);
        foreach (var chapter in chapters.Where(c => c.Images?.Count > 0))
        {
            foreach (var img in chapter.Images!)
            {
                if (!cacheManager.ImageExists(img.ImageId))
                {
                    imageUrls[img.ImageId] = img.Url;
                }
            }
        }

        if (imageUrls.Count == 0)
        {
            return;
        }

        _logger?.LogInformation("需要下载 {Count} 张图片", imageUrls.Count);

        var completed = 0;
        foreach (var (imageId, url) in imageUrls)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var data = await _client.DownloadImageAsync(url, cancellationToken).ConfigureAwait(false);
                await cacheManager.SaveImageAsync(imageId, data).ConfigureAwait(false);
                stats.ImagesDownloaded++;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "下载图片失败: {ImageId} {Url}", imageId, url);
            }

            completed++;
            var percentage = completed * 100.0 / imageUrls.Count;
            progress?.Report(SyncProgress.DownloadingImages(percentage, $"下载图片 {completed}/{imageUrls.Count}"));
        }
    }

    private async Task<string> GenerateEpubAsync(
        string bookId,
        BookDetail bookDetail,
        List<ChapterItem> allChapters,
        List<ChapterItem> lockedChapters,
        CacheManager cacheManager,
        EpubBook? existingBook,
        FanQieBookInfo? existingInfo,
        string tocHash,
        SyncOptions options,
        SyncStatisticsBuilder stats,
        IProgress<SyncProgress>? progress,
        CancellationToken cancellationToken)
    {
        var chapterInfos = new List<ChapterInfo>();
        var cachedChapters = await cacheManager.LoadAllChaptersAsync().ConfigureAwait(false);
        var cachedDict = cachedChapters.ToDictionary(c => c.ChapterId);
        var existingIds = existingInfo?.DownloadedChapterIds.ToHashSet() ?? [];

        var processedCount = 0;
        var totalCount = allChapters.Count;

        foreach (var chapter in allChapters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string htmlContent;
            var images = new List<ChapterImageInfo>();

            // 锁定章节
            if (lockedChapters.Any(c => c.ItemId == chapter.ItemId))
            {
                htmlContent = PlaceholderGenerator.GenerateLockedPlaceholder(
                    chapter.ItemId,
                    chapter.Title,
                    chapter.Order);
            }
            // 从缓存获取
            else if (cachedDict.TryGetValue(chapter.ItemId, out var cached))
            {
                if (cached.Status == ChapterStatus.Downloaded && !string.IsNullOrEmpty(cached.HtmlContent))
                {
                    htmlContent = PlaceholderGenerator.WrapChapterContent(
                        chapter.ItemId,
                        chapter.Title,
                        chapter.Order,
                        cached.HtmlContent);

                    // 加载图片数据（EpubGenerator 会根据 ID 匹配占位符替换）
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
                        chapter.ItemId,
                        chapter.Title,
                        chapter.Order,
                        cached.FailureReason);
                }
            }
            // 从现有 EPUB 复用
            else if (existingBook != null && existingIds.Contains(chapter.ItemId))
            {
                var existingChapter = await FanQieEpubAnalyzer.ReadChapterContentAsync(existingBook, chapter.ItemId).ConfigureAwait(false);
                if (existingChapter != null && !string.IsNullOrEmpty(existingChapter.BodyContent))
                {
                    // 使用 body 内部内容，不需要再次包装（内容已包含元数据标记）
                    htmlContent = existingChapter.BodyContent;

                    // 复用现有 EPUB 中的图片
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
                        chapter.ItemId,
                        chapter.Title,
                        chapter.Order,
                        "无法从现有 EPUB 读取");
                }
            }
            // 失败章节
            else
            {
                htmlContent = PlaceholderGenerator.GenerateFailedPlaceholder(
                    chapter.ItemId,
                    chapter.Title,
                    chapter.Order);

                if (!stats.FailedChapterIds.Contains(chapter.ItemId))
                {
                    stats.FailedChapterIds.Add(chapter.ItemId);
                    stats.Failed++;
                }
            }

            chapterInfos.Add(new ChapterInfo
            {
                Index = chapter.Order,
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

        // 不再需要现有 EPUB，提前释放以避免文件锁
        // 这样可以正常覆盖同一个输出文件
        existingBook?.Dispose();

        // 准备封面
        CoverInfo? cover = null;
        var coverData = await cacheManager.LoadImageAsync("cover").ConfigureAwait(false);
        if (coverData != null)
        {
            cover = CoverInfo.FromBytes(coverData, GuessImageMediaType(bookDetail.CoverUrl ?? "cover.jpg"));
        }

        // 准备元数据
        var metadata = new EpubMetadata
        {
            Title = bookDetail.Title,
            Author = bookDetail.Author,
            Description = bookDetail.Abstract,
            Language = "zh",
            Identifier = $"fanqie-{bookId}",
            Subjects = bookDetail.Tags?.ToList(),
            Cover = cover,
            CustomMetadata =
            [
                CustomMetadata.Create("fanqie:book-id", bookId),
                CustomMetadata.Create("fanqie:sync-time", DateTimeOffset.Now.ToString("O")),
                CustomMetadata.Create("fanqie:toc-hash", tocHash),
                CustomMetadata.Create("fanqie:chapter-count", allChapters.Count.ToString()),
                .. (stats.FailedChapterIds.Count > 0
                    ? [CustomMetadata.Create("fanqie:failed-chapters", string.Join(",", stats.FailedChapterIds))]
                    : Array.Empty<CustomMetadata>()),
            ],
        };

        // 生成 EPUB
        var epubFileName = $"{bookId}.epub";
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
        int completed,
        int total,
        int failed,
        int skipped,
        string currentChapter)
    {
        progress?.Report(SyncProgress.DownloadingChapters(new DownloadProgressDetail
        {
            Completed = completed,
            Total = total,
            Failed = failed,
            Skipped = skipped,
            CurrentChapter = currentChapter,
        }));
    }

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

    /// <summary>
    /// 同步统计构建器.
    /// </summary>
    private sealed class SyncStatisticsBuilder
    {
        public int TotalChapters { get; set; }
        public int NewlyDownloaded { get; set; }
        public int Reused { get; set; }
        public int Failed { get; set; }
        public int RestoredFromCache { get; set; }
        public int ImagesDownloaded { get; set; }
        public int LockedChapters { get; set; }
        public TimeSpan Duration { get; set; }
        public List<string> FailedChapterIds { get; } = [];

        public SyncStatistics Build() => new()
        {
            TotalChapters = TotalChapters,
            NewlyDownloaded = NewlyDownloaded,
            Reused = Reused,
            Failed = Failed,
            RestoredFromCache = RestoredFromCache,
            ImagesDownloaded = ImagesDownloaded,
            LockedChapters = LockedChapters,
            Duration = Duration,
        };
    }
}

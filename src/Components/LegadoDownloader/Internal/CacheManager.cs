// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.Legado.Internal;

/// <summary>
/// 缓存管理器，管理下载过程中的临时缓存.
/// </summary>
internal sealed class CacheManager
{
    private const string ManifestFileName = "manifest.json";
    private const string ChaptersFolder = "chapters";
    private const string ImagesFolder = "images";

    private readonly string _tempDirectory;
    private readonly string _bookUrlHash;
    private readonly string _cacheRoot;
    private readonly SemaphoreSlim _manifestLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// 初始化 <see cref="CacheManager"/> 类的新实例.
    /// </summary>
    /// <param name="tempDirectory">临时目录根路径.</param>
    /// <param name="bookUrl">书籍链接.</param>
    public CacheManager(string tempDirectory, string bookUrl)
    {
        _tempDirectory = tempDirectory;
        _bookUrlHash = ComputeBookUrlHash(bookUrl);
        _cacheRoot = Path.Combine(tempDirectory, $"legado_{_bookUrlHash}");
    }

    /// <summary>
    /// 获取缓存根目录.
    /// </summary>
    public string CacheRoot => _cacheRoot;

    /// <summary>
    /// 获取章节缓存目录.
    /// </summary>
    public string ChaptersDirectory => Path.Combine(_cacheRoot, ChaptersFolder);

    /// <summary>
    /// 获取图片缓存目录.
    /// </summary>
    public string ImagesDirectory => Path.Combine(_cacheRoot, ImagesFolder);

    /// <summary>
    /// 初始化缓存目录.
    /// </summary>
    /// <param name="bookUrl">书籍链接.</param>
    /// <param name="tocHash">目录哈希.</param>
    /// <param name="title">书籍标题.</param>
    /// <param name="bookSource">书源链接.</param>
    /// <param name="serverUrl">服务地址.</param>
    public async Task InitializeAsync(string bookUrl, string tocHash, string? title = null, string? bookSource = null, string? serverUrl = null)
    {
        Directory.CreateDirectory(_cacheRoot);
        Directory.CreateDirectory(ChaptersDirectory);
        Directory.CreateDirectory(ImagesDirectory);

        var manifest = new CacheManifest
        {
            BookUrl = bookUrl,
            Title = title,
            BookSource = bookSource,
            ServerUrl = serverUrl,
            TocHash = tocHash,
            CreatedAt = DateTimeOffset.Now,
            UpdatedAt = DateTimeOffset.Now,
        };

        await _manifestLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await SaveManifestInternalAsync(manifest).ConfigureAwait(false);
        }
        finally
        {
            _manifestLock.Release();
        }
    }

    /// <summary>
    /// 检查缓存是否存在.
    /// </summary>
    public bool Exists()
        => Directory.Exists(_cacheRoot) && File.Exists(Path.Combine(_cacheRoot, ManifestFileName));

    /// <summary>
    /// 读取缓存清单.
    /// </summary>
    public async Task<CacheManifest?> LoadManifestAsync()
    {
        await _manifestLock.WaitAsync().ConfigureAwait(false);
        try
        {
            return await LoadManifestInternalAsync().ConfigureAwait(false);
        }
        finally
        {
            _manifestLock.Release();
        }
    }

    /// <summary>
    /// 保存缓存清单.
    /// </summary>
    public async Task SaveManifestAsync(CacheManifest manifest)
    {
        await _manifestLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await SaveManifestInternalAsync(manifest).ConfigureAwait(false);
        }
        finally
        {
            _manifestLock.Release();
        }
    }

    /// <summary>
    /// 读取缓存清单（内部方法，调用方需要持有锁）.
    /// </summary>
    private async Task<CacheManifest?> LoadManifestInternalAsync()
    {
        var manifestPath = Path.Combine(_cacheRoot, ManifestFileName);
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(manifestPath).ConfigureAwait(false);
            return JsonSerializer.Deserialize<CacheManifest>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 保存缓存清单（内部方法，调用方需要持有锁）.
    /// </summary>
    private async Task SaveManifestInternalAsync(CacheManifest manifest)
    {
        var manifestPath = Path.Combine(_cacheRoot, ManifestFileName);
        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        await File.WriteAllTextAsync(manifestPath, json).ConfigureAwait(false);
    }

    /// <summary>
    /// 保存章节.
    /// </summary>
    public async Task SaveChapterAsync(CachedChapter chapter)
    {
        var chapterPath = Path.Combine(ChaptersDirectory, $"{chapter.ChapterIndex}.json");
        var json = JsonSerializer.Serialize(chapter, JsonOptions);
        await File.WriteAllTextAsync(chapterPath, json).ConfigureAwait(false);

        // 更新清单（需要加锁保护）
        await _manifestLock.WaitAsync().ConfigureAwait(false);
        try
        {
            var manifest = await LoadManifestInternalAsync().ConfigureAwait(false);
            if (manifest != null)
            {
                if (chapter.Status == ChapterStatus.Downloaded || chapter.Status == ChapterStatus.Volume)
                {
                    if (!manifest.CachedChapterIndexes.Contains(chapter.ChapterIndex))
                    {
                        manifest.CachedChapterIndexes.Add(chapter.ChapterIndex);
                    }

                    manifest.FailedChapterIndexes.Remove(chapter.ChapterIndex);
                }
                else if (chapter.Status == ChapterStatus.Failed)
                {
                    if (!manifest.FailedChapterIndexes.Contains(chapter.ChapterIndex))
                    {
                        manifest.FailedChapterIndexes.Add(chapter.ChapterIndex);
                    }
                }

                manifest.UpdatedAt = DateTimeOffset.Now;
                await SaveManifestInternalAsync(manifest).ConfigureAwait(false);
            }
        }
        finally
        {
            _manifestLock.Release();
        }
    }

    /// <summary>
    /// 读取章节.
    /// </summary>
    public async Task<CachedChapter?> LoadChapterAsync(int chapterIndex)
    {
        var chapterPath = Path.Combine(ChaptersDirectory, $"{chapterIndex}.json");
        if (!File.Exists(chapterPath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(chapterPath).ConfigureAwait(false);
            return JsonSerializer.Deserialize<CachedChapter>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取所有已缓存的章节.
    /// </summary>
    public async Task<List<CachedChapter>> LoadAllChaptersAsync()
    {
        var chapters = new List<CachedChapter>();

        if (!Directory.Exists(ChaptersDirectory))
        {
            return chapters;
        }

        foreach (var file in Directory.GetFiles(ChaptersDirectory, "*.json"))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file).ConfigureAwait(false);
                var chapter = JsonSerializer.Deserialize<CachedChapter>(json, JsonOptions);
                if (chapter != null)
                {
                    chapters.Add(chapter);
                }
            }
            catch
            {
                // 忽略损坏的文件
            }
        }

        return chapters;
    }

    /// <summary>
    /// 保存图片.
    /// </summary>
    public async Task SaveImageAsync(string imageId, byte[] data)
    {
        var imagePath = Path.Combine(ImagesDirectory, imageId);
        await File.WriteAllBytesAsync(imagePath, data).ConfigureAwait(false);
    }

    /// <summary>
    /// 读取图片.
    /// </summary>
    public async Task<byte[]?> LoadImageAsync(string imageId)
    {
        var imagePath = Path.Combine(ImagesDirectory, imageId);
        if (!File.Exists(imagePath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(imagePath).ConfigureAwait(false);
    }

    /// <summary>
    /// 检查图片是否存在.
    /// </summary>
    public bool ImageExists(string imageId)
        => File.Exists(Path.Combine(ImagesDirectory, imageId));

    /// <summary>
    /// 获取所有已缓存图片的 ID 列表.
    /// </summary>
    public IEnumerable<string> GetCachedImageIds()
    {
        if (!Directory.Exists(ImagesDirectory))
        {
            return [];
        }

        return Directory.GetFiles(ImagesDirectory)
            .Select(Path.GetFileName)
            .Where(f => f != null)!;
    }

    /// <summary>
    /// 清理缓存.
    /// </summary>
    public void Cleanup()
    {
        if (Directory.Exists(_cacheRoot))
        {
            try
            {
                Directory.Delete(_cacheRoot, recursive: true);
            }
            catch
            {
                // 忽略清理失败
            }
        }
    }

    /// <summary>
    /// 获取缓存状态.
    /// </summary>
    public async Task<CacheState?> GetStateAsync()
    {
        await _manifestLock.WaitAsync().ConfigureAwait(false);
        try
        {
            var manifest = await LoadManifestInternalAsync().ConfigureAwait(false);
            if (manifest == null)
            {
                return null;
            }

            return new CacheState
            {
                BookUrl = manifest.BookUrl,
                Title = manifest.Title,
                BookSource = manifest.BookSource,
                ServerUrl = manifest.ServerUrl,
                TocHash = manifest.TocHash,
                CachedChapterIndexes = manifest.CachedChapterIndexes,
                FailedChapterIndexes = manifest.FailedChapterIndexes,
                CreatedAt = manifest.CreatedAt,
                UpdatedAt = manifest.UpdatedAt,
            };
        }
        finally
        {
            _manifestLock.Release();
        }
    }

    /// <summary>
    /// 计算书籍链接的哈希值（用于缓存目录名）.
    /// </summary>
    private static string ComputeBookUrlHash(string bookUrl)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(bookUrl));
        return Convert.ToHexString(hash)[..32].ToLowerInvariant();
    }
}

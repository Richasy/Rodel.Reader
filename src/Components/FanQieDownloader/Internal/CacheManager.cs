// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Internal;

/// <summary>
/// 缓存管理器，管理下载过程中的临时缓存.
/// </summary>
internal sealed class CacheManager
{
    private const string ManifestFileName = "manifest.json";
    private const string ChaptersFolder = "chapters";
    private const string ImagesFolder = "images";

    private readonly string _tempDirectory;
    private readonly string _bookId;
    private readonly string _cacheRoot;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// 初始化 <see cref="CacheManager"/> 类的新实例.
    /// </summary>
    /// <param name="tempDirectory">临时目录根路径.</param>
    /// <param name="bookId">书籍 ID.</param>
    public CacheManager(string tempDirectory, string bookId)
    {
        _tempDirectory = tempDirectory;
        _bookId = bookId;
        _cacheRoot = Path.Combine(tempDirectory, $"fanqie_{bookId}");
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
    /// <param name="tocHash">目录哈希.</param>
    /// <param name="title">书籍标题.</param>
    public async Task InitializeAsync(string tocHash, string? title = null)
    {
        Directory.CreateDirectory(_cacheRoot);
        Directory.CreateDirectory(ChaptersDirectory);
        Directory.CreateDirectory(ImagesDirectory);

        var manifest = new CacheManifest
        {
            BookId = _bookId,
            Title = title,
            TocHash = tocHash,
            CreatedAt = DateTimeOffset.Now,
            UpdatedAt = DateTimeOffset.Now,
        };

        await SaveManifestAsync(manifest).ConfigureAwait(false);
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
    /// 保存缓存清单.
    /// </summary>
    public async Task SaveManifestAsync(CacheManifest manifest)
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
        var chapterPath = Path.Combine(ChaptersDirectory, $"{chapter.ChapterId}.json");
        var json = JsonSerializer.Serialize(chapter, JsonOptions);
        await File.WriteAllTextAsync(chapterPath, json).ConfigureAwait(false);

        // 更新清单
        var manifest = await LoadManifestAsync().ConfigureAwait(false);
        if (manifest != null)
        {
            if (chapter.Status == ChapterStatus.Downloaded)
            {
                if (!manifest.CachedChapterIds.Contains(chapter.ChapterId))
                {
                    manifest.CachedChapterIds.Add(chapter.ChapterId);
                }

                manifest.FailedChapterIds.Remove(chapter.ChapterId);
            }
            else if (chapter.Status == ChapterStatus.Failed)
            {
                if (!manifest.FailedChapterIds.Contains(chapter.ChapterId))
                {
                    manifest.FailedChapterIds.Add(chapter.ChapterId);
                }
            }

            manifest.UpdatedAt = DateTimeOffset.Now;
            await SaveManifestAsync(manifest).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 读取章节.
    /// </summary>
    public async Task<CachedChapter?> LoadChapterAsync(string chapterId)
    {
        var chapterPath = Path.Combine(ChaptersDirectory, $"{chapterId}.json");
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
        var manifest = await LoadManifestAsync().ConfigureAwait(false);
        if (manifest == null)
        {
            return null;
        }

        return new CacheState
        {
            BookId = manifest.BookId,
            Title = manifest.Title,
            TocHash = manifest.TocHash,
            CachedChapterIds = manifest.CachedChapterIds,
            FailedChapterIds = manifest.FailedChapterIds,
            CreatedAt = manifest.CreatedAt,
            UpdatedAt = manifest.UpdatedAt,
        };
    }
}

// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// EPUB 构建器实现.
/// </summary>
/// <remarks>
/// 此构建器负责协调所有生成器，生成 EPUB 内容并打包为文件.
/// </remarks>
public sealed class EpubBuilder : IEpubBuilder
{
    private const string MimeTypeContent = "application/epub+zip";

    private readonly IContainerGenerator _containerGenerator;
    private readonly IOpfGenerator _opfGenerator;
    private readonly INcxGenerator _ncxGenerator;
    private readonly INavDocGenerator _navDocGenerator;
    private readonly IStyleSheetGenerator _styleSheetGenerator;
    private readonly ICoverPageGenerator _coverPageGenerator;
    private readonly ITitlePageGenerator _titlePageGenerator;
    private readonly ITocPageGenerator _tocPageGenerator;
    private readonly ICopyrightPageGenerator _copyrightPageGenerator;
    private readonly IChapterGenerator _chapterGenerator;
    private readonly IEpubPackager _packager;

    /// <summary>
    /// 初始化 <see cref="EpubBuilder"/> 类的新实例.
    /// </summary>
    public EpubBuilder()
        : this(
            new ContainerGenerator(),
            new OpfGenerator(),
            new NcxGenerator(),
            new NavDocGenerator(),
            new StyleSheetGenerator(),
            new CoverPageGenerator(),
            new TitlePageGenerator(),
            new TocPageGenerator(),
            new CopyrightPageGenerator(),
            new ChapterGenerator(),
            new ZipEpubPackager())
    {
    }

    /// <summary>
    /// 初始化 <see cref="EpubBuilder"/> 类的新实例（依赖注入）.
    /// </summary>
    public EpubBuilder(
        IContainerGenerator containerGenerator,
        IOpfGenerator opfGenerator,
        INcxGenerator ncxGenerator,
        INavDocGenerator navDocGenerator,
        IStyleSheetGenerator styleSheetGenerator,
        ICoverPageGenerator coverPageGenerator,
        ITitlePageGenerator titlePageGenerator,
        ITocPageGenerator tocPageGenerator,
        ICopyrightPageGenerator copyrightPageGenerator,
        IChapterGenerator chapterGenerator,
        IEpubPackager packager)
    {
        _containerGenerator = containerGenerator ?? throw new ArgumentNullException(nameof(containerGenerator));
        _opfGenerator = opfGenerator ?? throw new ArgumentNullException(nameof(opfGenerator));
        _ncxGenerator = ncxGenerator ?? throw new ArgumentNullException(nameof(ncxGenerator));
        _navDocGenerator = navDocGenerator ?? throw new ArgumentNullException(nameof(navDocGenerator));
        _styleSheetGenerator = styleSheetGenerator ?? throw new ArgumentNullException(nameof(styleSheetGenerator));
        _coverPageGenerator = coverPageGenerator ?? throw new ArgumentNullException(nameof(coverPageGenerator));
        _titlePageGenerator = titlePageGenerator ?? throw new ArgumentNullException(nameof(titlePageGenerator));
        _tocPageGenerator = tocPageGenerator ?? throw new ArgumentNullException(nameof(tocPageGenerator));
        _copyrightPageGenerator = copyrightPageGenerator ?? throw new ArgumentNullException(nameof(copyrightPageGenerator));
        _chapterGenerator = chapterGenerator ?? throw new ArgumentNullException(nameof(chapterGenerator));
        _packager = packager ?? throw new ArgumentNullException(nameof(packager));
    }

    /// <inheritdoc/>
    public async Task BuildAsync(
        EpubMetadata metadata,
        IReadOnlyList<ChapterInfo> chapters,
        Stream outputStream,
        EpubOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var content = GenerateContent(metadata, chapters, options);
        await _packager.PackageAsync(content, outputStream, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task BuildToFileAsync(
        EpubMetadata metadata,
        IReadOnlyList<ChapterInfo> chapters,
        string outputPath,
        EpubOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var content = GenerateContent(metadata, chapters, options);
        await _packager.PackageToFileAsync(content, outputPath, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<byte[]> BuildToBytesAsync(
        EpubMetadata metadata,
        IReadOnlyList<ChapterInfo> chapters,
        EpubOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var content = GenerateContent(metadata, chapters, options);
        return await _packager.PackageToBytesAsync(content, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public EpubContent GenerateContent(
        EpubMetadata metadata,
        IReadOnlyList<ChapterInfo> chapters,
        EpubOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(chapters);

        if (chapters.Count == 0)
        {
            throw new ArgumentException("章节列表不能为空。", nameof(chapters));
        }

        options ??= new EpubOptions();

        // 生成各个组件
        var containerXml = _containerGenerator.Generate();
        var contentOpf = _opfGenerator.Generate(metadata, chapters, options);
        var tocNcx = _ncxGenerator.Generate(metadata, chapters);
        var styleSheet = GenerateStyleSheet(options);
        var titlePage = _titlePageGenerator.Generate(metadata);

        // 可选组件
        var navDoc = options.Version == EpubVersion.Epub3
            ? _navDocGenerator.Generate(metadata, chapters)
            : null;

        var coverPage = metadata.Cover != null
            ? _coverPageGenerator.Generate(metadata.Cover, metadata.Title)
            : null;

        var tocPage = options.IncludeTocPage
            ? _tocPageGenerator.Generate(chapters, metadata.Title)
            : null;

        var copyrightPage = options.IncludeCopyrightPage && metadata.Copyright != null
            ? _copyrightPageGenerator.Generate(metadata)
            : null;

        // 生成章节内容
        var chapterContents = GenerateChapterContents(chapters, metadata.Title);

        // 收集所有章节图片
        var chapterImages = CollectChapterImages(chapters);

        return new EpubContent
        {
            Mimetype = MimeTypeContent,
            ContainerXml = containerXml,
            ContentOpf = contentOpf,
            TocNcx = tocNcx,
            NavDoc = navDoc,
            CoverPage = coverPage,
            TitlePage = titlePage,
            TocPage = tocPage,
            CopyrightPage = copyrightPage,
            StyleSheet = styleSheet,
            Chapters = chapterContents,
            Cover = metadata.Cover,
            ChapterImages = chapterImages,
            Resources = options.Resources,
        };
    }

    private static List<ChapterImageInfo>? CollectChapterImages(IReadOnlyList<ChapterInfo> chapters)
    {
        List<ChapterImageInfo>? allImages = null;

        foreach (var chapter in chapters)
        {
            if (chapter.Images is { Count: > 0 })
            {
                allImages ??= [];
                allImages.AddRange(chapter.Images);
            }
        }

        return allImages;
    }

    private string GenerateStyleSheet(EpubOptions options)
    {
        var defaultCss = _styleSheetGenerator.Generate(options);

        if (string.IsNullOrEmpty(options.CustomCss))
        {
            return defaultCss;
        }

        // 追加自定义 CSS
        var sb = StringBuilderPool.Rent();
        sb.AppendLine(defaultCss);
        sb.AppendLine();
        sb.AppendLine("/* Custom Styles */");
        sb.AppendLine(options.CustomCss);
        return StringBuilderPool.ToStringAndReturn(sb);
    }

    private Dictionary<string, string> GenerateChapterContents(IReadOnlyList<ChapterInfo> chapters, string bookTitle)
    {
        var result = new Dictionary<string, string>(chapters.Count, StringComparer.Ordinal);

        foreach (var chapter in chapters)
        {
            var fileName = $"{chapter.FileName}.xhtml";
            var content = _chapterGenerator.Generate(chapter);
            result[fileName] = content;
        }

        return result;
    }
}

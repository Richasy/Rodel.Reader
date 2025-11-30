// Copyright (c) Reader Copilot. All rights reserved.

namespace EpubGenerator.Test.Builder;

[TestClass]
public sealed class EpubBuilderTests
{
    private Mock<IContainerGenerator> _containerGeneratorMock = null!;
    private Mock<IOpfGenerator> _opfGeneratorMock = null!;
    private Mock<INcxGenerator> _ncxGeneratorMock = null!;
    private Mock<INavDocGenerator> _navDocGeneratorMock = null!;
    private Mock<IStyleSheetGenerator> _styleSheetGeneratorMock = null!;
    private Mock<ICoverPageGenerator> _coverPageGeneratorMock = null!;
    private Mock<ITitlePageGenerator> _titlePageGeneratorMock = null!;
    private Mock<ITocPageGenerator> _tocPageGeneratorMock = null!;
    private Mock<ICopyrightPageGenerator> _copyrightPageGeneratorMock = null!;
    private Mock<IChapterGenerator> _chapterGeneratorMock = null!;
    private Mock<IEpubPackager> _packagerMock = null!;
    private EpubBuilder _builder = null!;

    [TestInitialize]
    public void Setup()
    {
        _containerGeneratorMock = new Mock<IContainerGenerator>(MockBehavior.Strict);
        _opfGeneratorMock = new Mock<IOpfGenerator>(MockBehavior.Strict);
        _ncxGeneratorMock = new Mock<INcxGenerator>(MockBehavior.Strict);
        _navDocGeneratorMock = new Mock<INavDocGenerator>(MockBehavior.Strict);
        _styleSheetGeneratorMock = new Mock<IStyleSheetGenerator>(MockBehavior.Strict);
        _coverPageGeneratorMock = new Mock<ICoverPageGenerator>(MockBehavior.Strict);
        _titlePageGeneratorMock = new Mock<ITitlePageGenerator>(MockBehavior.Strict);
        _tocPageGeneratorMock = new Mock<ITocPageGenerator>(MockBehavior.Strict);
        _copyrightPageGeneratorMock = new Mock<ICopyrightPageGenerator>(MockBehavior.Strict);
        _chapterGeneratorMock = new Mock<IChapterGenerator>(MockBehavior.Strict);
        _packagerMock = new Mock<IEpubPackager>(MockBehavior.Strict);

        // 设置默认返回值
        _containerGeneratorMock.Setup(g => g.Generate()).Returns("<container/>");
        _opfGeneratorMock.Setup(g => g.Generate(It.IsAny<EpubMetadata>(), It.IsAny<IReadOnlyList<ChapterInfo>>(), It.IsAny<EpubOptions>()))
            .Returns("<package/>");
        _ncxGeneratorMock.Setup(g => g.Generate(It.IsAny<EpubMetadata>(), It.IsAny<IReadOnlyList<ChapterInfo>>()))
            .Returns("<ncx/>");
        _navDocGeneratorMock.Setup(g => g.Generate(It.IsAny<EpubMetadata>(), It.IsAny<IReadOnlyList<ChapterInfo>>()))
            .Returns("<nav/>");
        _styleSheetGeneratorMock.Setup(g => g.Generate(It.IsAny<EpubOptions>()))
            .Returns("body {}");
        _titlePageGeneratorMock.Setup(g => g.Generate(It.IsAny<EpubMetadata>()))
            .Returns("<titlepage/>");
        _tocPageGeneratorMock.Setup(g => g.Generate(It.IsAny<IReadOnlyList<ChapterInfo>>(), It.IsAny<string?>()))
            .Returns("<toc/>");
        _copyrightPageGeneratorMock.Setup(g => g.Generate(It.IsAny<EpubMetadata>()))
            .Returns("<copyright/>");
        _chapterGeneratorMock.Setup(g => g.Generate(It.IsAny<ChapterInfo>()))
            .Returns("<chapter/>");

        _builder = new EpubBuilder(
            _containerGeneratorMock.Object,
            _opfGeneratorMock.Object,
            _ncxGeneratorMock.Object,
            _navDocGeneratorMock.Object,
            _styleSheetGeneratorMock.Object,
            _coverPageGeneratorMock.Object,
            _titlePageGeneratorMock.Object,
            _tocPageGeneratorMock.Object,
            _copyrightPageGeneratorMock.Object,
            _chapterGeneratorMock.Object,
            _packagerMock.Object);
    }

    [TestMethod]
    public void GenerateContent_ShouldCallContainerGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        _builder.GenerateContent(metadata, chapters);

        // Assert
        _containerGeneratorMock.Verify(g => g.Generate(), Times.Once);
    }

    [TestMethod]
    public void GenerateContent_ShouldCallOpfGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        _builder.GenerateContent(metadata, chapters);

        // Assert
        _opfGeneratorMock.Verify(g => g.Generate(metadata, chapters, It.IsAny<EpubOptions>()), Times.Once);
    }

    [TestMethod]
    public void GenerateContent_ShouldCallNcxGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        _builder.GenerateContent(metadata, chapters);

        // Assert
        _ncxGeneratorMock.Verify(g => g.Generate(metadata, chapters), Times.Once);
    }

    [TestMethod]
    public void GenerateContent_ShouldCallTitlePageGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        _builder.GenerateContent(metadata, chapters);

        // Assert
        _titlePageGeneratorMock.Verify(g => g.Generate(metadata), Times.Once);
    }

    [TestMethod]
    public void GenerateContent_ShouldCallChapterGeneratorForEachChapter()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters(3);

        // Act
        _builder.GenerateContent(metadata, chapters);

        // Assert
        _chapterGeneratorMock.Verify(g => g.Generate(It.IsAny<ChapterInfo>()), Times.Exactly(3));
    }

    [TestMethod]
    public void GenerateContent_WithEpub3_ShouldCallNavDocGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();
        var options = new EpubOptions { Version = EpubVersion.Epub3 };

        // Act
        _builder.GenerateContent(metadata, chapters, options);

        // Assert
        _navDocGeneratorMock.Verify(g => g.Generate(metadata, chapters), Times.Once);
    }

    [TestMethod]
    public void GenerateContent_WithEpub2_ShouldNotCallNavDocGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();
        var options = new EpubOptions { Version = EpubVersion.Epub2 };

        // Act
        _builder.GenerateContent(metadata, chapters, options);

        // Assert
        _navDocGeneratorMock.Verify(g => g.Generate(It.IsAny<EpubMetadata>(), It.IsAny<IReadOnlyList<ChapterInfo>>()), Times.Never);
    }

    [TestMethod]
    public void GenerateContent_WithCover_ShouldCallCoverPageGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateMetadataWithCover();
        var chapters = TestDataFactory.CreateChapters();
        _coverPageGeneratorMock.Setup(g => g.Generate(It.IsAny<CoverInfo>(), It.IsAny<string>()))
            .Returns("<cover/>");

        // Act
        _builder.GenerateContent(metadata, chapters);

        // Assert
        _coverPageGeneratorMock.Verify(g => g.Generate(It.IsAny<CoverInfo>(), metadata.Title), Times.Once);
    }

    [TestMethod]
    public void GenerateContent_WithoutCover_ShouldNotCallCoverPageGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        _builder.GenerateContent(metadata, chapters);

        // Assert
        _coverPageGeneratorMock.Verify(g => g.Generate(It.IsAny<CoverInfo>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void GenerateContent_WithIncludeTocPage_ShouldCallTocPageGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();
        var options = new EpubOptions { IncludeTocPage = true };

        // Act
        _builder.GenerateContent(metadata, chapters, options);

        // Assert
        _tocPageGeneratorMock.Verify(g => g.Generate(chapters, It.IsAny<string?>()), Times.Once);
    }

    [TestMethod]
    public void GenerateContent_WithoutIncludeTocPage_ShouldNotCallTocPageGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();
        var options = new EpubOptions { IncludeTocPage = false };

        // Act
        _builder.GenerateContent(metadata, chapters, options);

        // Assert
        _tocPageGeneratorMock.Verify(g => g.Generate(It.IsAny<IReadOnlyList<ChapterInfo>>(), It.IsAny<string?>()), Times.Never);
    }

    [TestMethod]
    public void GenerateContent_WithCopyrightAndIncludeCopyrightPage_ShouldCallCopyrightPageGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateMetadataWithCopyright();
        var chapters = TestDataFactory.CreateChapters();
        var options = new EpubOptions { IncludeCopyrightPage = true };

        // Act
        _builder.GenerateContent(metadata, chapters, options);

        // Assert
        _copyrightPageGeneratorMock.Verify(g => g.Generate(metadata), Times.Once);
    }

    [TestMethod]
    public void GenerateContent_WithoutCopyrightInfo_ShouldNotCallCopyrightPageGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();
        var options = new EpubOptions { IncludeCopyrightPage = true };

        // Act
        _builder.GenerateContent(metadata, chapters, options);

        // Assert
        _copyrightPageGeneratorMock.Verify(g => g.Generate(It.IsAny<EpubMetadata>()), Times.Never);
    }

    [TestMethod]
    public void GenerateContent_ShouldReturnEpubContentWithCorrectMimetype()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _builder.GenerateContent(metadata, chapters);

        // Assert
        Assert.AreEqual("application/epub+zip", result.Mimetype);
    }

    [TestMethod]
    public void GenerateContent_ShouldReturnEpubContentWithChapters()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters(2);

        // Act
        var result = _builder.GenerateContent(metadata, chapters);

        // Assert
        Assert.AreEqual(2, result.Chapters.Count);
    }

    [TestMethod]
    public void GenerateContent_WithNullMetadata_ShouldThrowArgumentNullException()
    {
        // Arrange
        var chapters = TestDataFactory.CreateChapters();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => _builder.GenerateContent(null!, chapters));
    }

    [TestMethod]
    public void GenerateContent_WithNullChapters_ShouldThrowArgumentNullException()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => _builder.GenerateContent(metadata, null!));
    }

    [TestMethod]
    public void GenerateContent_WithEmptyChapters_ShouldThrowArgumentException()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = Array.Empty<ChapterInfo>();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => _builder.GenerateContent(metadata, chapters));
    }

    [TestMethod]
    public async Task BuildAsync_ShouldCallPackagerPackageAsync()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();
        using var stream = new MemoryStream();

        _packagerMock.Setup(p => p.PackageAsync(It.IsAny<EpubContent>(), stream, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _builder.BuildAsync(metadata, chapters, stream);

        // Assert
        _packagerMock.Verify(p => p.PackageAsync(It.IsAny<EpubContent>(), stream, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task BuildToFileAsync_ShouldCallPackagerPackageToFileAsync()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();
        const string outputPath = "test.epub";

        _packagerMock.Setup(p => p.PackageToFileAsync(It.IsAny<EpubContent>(), outputPath, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _builder.BuildToFileAsync(metadata, chapters, outputPath);

        // Assert
        _packagerMock.Verify(p => p.PackageToFileAsync(It.IsAny<EpubContent>(), outputPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task BuildToBytesAsync_ShouldCallPackagerPackageToBytesAsync()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();
        _packagerMock.Setup(p => p.PackageToBytesAsync(It.IsAny<EpubContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([0x50, 0x4B]);

        // Act
        var result = await _builder.BuildToBytesAsync(metadata, chapters);

        // Assert
        Assert.IsNotNull(result);
        _packagerMock.Verify(p => p.PackageToBytesAsync(It.IsAny<EpubContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public void GenerateContent_WithChapterImages_ShouldIncludeImagesInContent()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = new List<ChapterInfo> { TestDataFactory.CreateChapterWithImages() };

        // Act
        var result = _builder.GenerateContent(metadata, chapters);

        // Assert
        Assert.IsNotNull(result.ChapterImages);
        Assert.IsTrue(result.ChapterImages.Count > 0, "Should have chapter images");
    }

    [TestMethod]
    public void DefaultConstructor_ShouldCreateValidInstance()
    {
        // Act
        var builder = new EpubBuilder();

        // Assert
        Assert.IsNotNull(builder);
    }

    [TestMethod]
    public void GenerateContent_WithStyleSheet_ShouldCallStyleSheetGenerator()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        _builder.GenerateContent(metadata, chapters);

        // Assert
        _styleSheetGeneratorMock.Verify(g => g.Generate(It.IsAny<EpubOptions>()), Times.Once);
    }
}

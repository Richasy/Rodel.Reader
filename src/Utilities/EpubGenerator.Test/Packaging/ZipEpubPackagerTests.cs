// Copyright (c) Reader Copilot. All rights reserved.

using System.IO.Compression;
using static EpubGenerator.Test.AssertExtensions;

namespace EpubGenerator.Test.Packaging;

[TestClass]
public sealed class ZipEpubPackagerTests
{
    private ZipEpubPackager _packager = null!;

    [TestInitialize]
    public void Setup()
    {
        _packager = new ZipEpubPackager();
    }

    [TestMethod]
    public async Task PackageToBytesAsync_ShouldReturnValidZipBytes()
    {
        // Arrange
        var content = CreateTestEpubContent();

        // Act
        var result = await _packager.PackageToBytesAsync(content);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0, "EPUB bytes should not be empty");

        // Verify it's a valid ZIP
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        Assert.IsTrue(archive.Entries.Count > 0, "Archive should have entries");
    }

    [TestMethod]
    public async Task PackageToBytesAsync_ShouldContainMimetype()
    {
        // Arrange
        var content = CreateTestEpubContent();

        // Act
        var result = await _packager.PackageToBytesAsync(content);

        // Assert
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var mimetypeEntry = archive.GetEntry("mimetype");
        Assert.IsNotNull(mimetypeEntry, "mimetype entry should exist");

        using var reader = new StreamReader(mimetypeEntry.Open());
        var mimetypeContent = await reader.ReadToEndAsync();
        Assert.AreEqual("application/epub+zip", mimetypeContent);
    }

    [TestMethod]
    public async Task PackageToBytesAsync_ShouldContainContainerXml()
    {
        // Arrange
        var content = CreateTestEpubContent();

        // Act
        var result = await _packager.PackageToBytesAsync(content);

        // Assert
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var containerEntry = archive.GetEntry("META-INF/container.xml");
        Assert.IsNotNull(containerEntry, "container.xml should exist");

        using var reader = new StreamReader(containerEntry.Open());
        var containerContent = await reader.ReadToEndAsync();
        ContainsText(containerContent, "container");
    }

    [TestMethod]
    public async Task PackageToBytesAsync_ShouldContainContentOpf()
    {
        // Arrange
        var content = CreateTestEpubContent();

        // Act
        var result = await _packager.PackageToBytesAsync(content);

        // Assert
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var opfEntry = archive.GetEntry("OEBPS/content.opf");
        Assert.IsNotNull(opfEntry, "content.opf should exist");
    }

    [TestMethod]
    public async Task PackageToBytesAsync_ShouldContainTocNcx()
    {
        // Arrange
        var content = CreateTestEpubContent();

        // Act
        var result = await _packager.PackageToBytesAsync(content);

        // Assert
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var ncxEntry = archive.GetEntry("OEBPS/toc.ncx");
        Assert.IsNotNull(ncxEntry, "toc.ncx should exist");
    }

    [TestMethod]
    public async Task PackageToBytesAsync_ShouldContainStylesheet()
    {
        // Arrange
        var content = CreateTestEpubContent();

        // Act
        var result = await _packager.PackageToBytesAsync(content);

        // Assert
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var cssEntry = archive.GetEntry("OEBPS/Styles/main.css");
        Assert.IsNotNull(cssEntry, "main.css should exist");
    }

    [TestMethod]
    public async Task PackageToBytesAsync_ShouldContainChapters()
    {
        // Arrange
        var content = CreateTestEpubContent();

        // Act
        var result = await _packager.PackageToBytesAsync(content);

        // Assert
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var chapter1Entry = archive.GetEntry("OEBPS/Text/chapter_0.xhtml");
        Assert.IsNotNull(chapter1Entry, "chapter_0.xhtml should exist");
    }

    [TestMethod]
    public async Task PackageToBytesAsync_WithCover_ShouldContainCoverPage()
    {
        // Arrange
        var content = CreateTestEpubContentWithCover();

        // Act
        var result = await _packager.PackageToBytesAsync(content);

        // Assert
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var coverPageEntry = archive.GetEntry("OEBPS/Text/cover.xhtml");
        Assert.IsNotNull(coverPageEntry, "cover.xhtml should exist");
    }

    [TestMethod]
    public async Task PackageToBytesAsync_WithCover_ShouldContainCoverImage()
    {
        // Arrange
        var content = CreateTestEpubContentWithCover();

        // Act
        var result = await _packager.PackageToBytesAsync(content);

        // Assert
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var coverImageEntry = archive.GetEntry("OEBPS/Images/cover.jpg");
        Assert.IsNotNull(coverImageEntry, "cover image should exist");
    }

    [TestMethod]
    public async Task PackageAsync_ShouldWriteToStream()
    {
        // Arrange
        var content = CreateTestEpubContent();
        using var outputStream = new MemoryStream();

        // Act
        await _packager.PackageAsync(content, outputStream);

        // Assert
        Assert.IsTrue(outputStream.Length > 0, "Stream should have content");
    }

    [TestMethod]
    public async Task PackageToFileAsync_ShouldCreateFile()
    {
        // Arrange
        var content = CreateTestEpubContent();
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.epub");

        try
        {
            // Act
            await _packager.PackageToFileAsync(content, tempPath);

            // Assert
            Assert.IsTrue(File.Exists(tempPath), "EPUB file should be created");
            var fileInfo = new FileInfo(tempPath);
            Assert.IsTrue(fileInfo.Length > 0, "File should have content");
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private static EpubContent CreateTestEpubContent()
    {
        return new EpubContent
        {
            Mimetype = "application/epub+zip",
            ContainerXml = """
                <?xml version="1.0" encoding="UTF-8"?>
                <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
                  <rootfiles>
                    <rootfile full-path="OEBPS/content.opf" media-type="application/oebps-package+xml"/>
                  </rootfiles>
                </container>
                """,
            ContentOpf = """
                <?xml version="1.0" encoding="UTF-8"?>
                <package xmlns="http://www.idpf.org/2007/opf" version="2.0" unique-identifier="BookId">
                  <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                    <dc:title>Test Book</dc:title>
                    <dc:identifier id="BookId">test-uuid</dc:identifier>
                    <dc:language>zh</dc:language>
                  </metadata>
                  <manifest>
                    <item id="ncx" href="toc.ncx" media-type="application/x-dtbncx+xml"/>
                    <item id="titlepage" href="titlepage.xhtml" media-type="application/xhtml+xml"/>
                    <item id="chapter_0" href="chapter_0.xhtml" media-type="application/xhtml+xml"/>
                    <item id="css" href="style.css" media-type="text/css"/>
                  </manifest>
                  <spine toc="ncx">
                    <itemref idref="titlepage"/>
                    <itemref idref="chapter_0"/>
                  </spine>
                </package>
                """,
            TocNcx = """
                <?xml version="1.0" encoding="UTF-8"?>
                <ncx xmlns="http://www.daisy.org/z3986/2005/ncx/" version="2005-1">
                  <head>
                    <meta name="dtb:uid" content="test-uuid"/>
                  </head>
                  <docTitle><text>Test Book</text></docTitle>
                  <navMap>
                    <navPoint id="np_0" playOrder="1">
                      <navLabel><text>Chapter 1</text></navLabel>
                      <content src="chapter_0.xhtml"/>
                    </navPoint>
                  </navMap>
                </ncx>
                """,
            TitlePage = """
                <?xml version="1.0" encoding="UTF-8"?>
                <html xmlns="http://www.w3.org/1999/xhtml">
                  <head><title>Test Book</title></head>
                  <body><h1>Test Book</h1></body>
                </html>
                """,
            StyleSheet = "body { margin: 1em; }",
            Chapters = new Dictionary<string, string>
            {
                ["chapter_0.xhtml"] = """
                    <?xml version="1.0" encoding="UTF-8"?>
                    <html xmlns="http://www.w3.org/1999/xhtml">
                      <head><title>Chapter 1</title></head>
                      <body><h2>Chapter 1</h2><p>Content</p></body>
                    </html>
                    """
            }
        };
    }

    private static EpubContent CreateTestEpubContentWithCover()
    {
        var basicContent = CreateTestEpubContent();
        return new EpubContent
        {
            Mimetype = basicContent.Mimetype,
            ContainerXml = basicContent.ContainerXml,
            ContentOpf = basicContent.ContentOpf,
            TocNcx = basicContent.TocNcx,
            TitlePage = basicContent.TitlePage,
            StyleSheet = basicContent.StyleSheet,
            Chapters = basicContent.Chapters,
            CoverPage = """
                <?xml version="1.0" encoding="UTF-8"?>
                <html xmlns="http://www.w3.org/1999/xhtml">
                  <head><title>Cover</title></head>
                  <body><img src="images/cover.jpg" alt="Cover"/></body>
                </html>
                """,
            Cover = TestDataFactory.CreateCoverInfo()
        };
    }
}

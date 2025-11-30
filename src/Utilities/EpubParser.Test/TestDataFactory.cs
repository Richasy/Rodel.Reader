// Copyright (c) Richasy. All rights reserved.

using System.IO.Compression;
using System.Text;

namespace EpubParser.Test;

/// <summary>
/// 测试数据工厂。
/// </summary>
internal static class TestDataFactory
{
    /// <summary>
    /// 创建最小的有效 EPUB 压缩包。
    /// </summary>
    public static MemoryStream CreateMinimalEpubStream()
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            // mimetype
            AddEntry(archive, "mimetype", "application/epub+zip");

            // container.xml
            AddEntry(archive, "META-INF/container.xml", ContainerXml);

            // content.opf
            AddEntry(archive, "OEBPS/content.opf", MinimalOpfXml);

            // nav.xhtml (EPUB 3)
            AddEntry(archive, "OEBPS/nav.xhtml", NavXhtml);

            // toc.ncx (EPUB 2)
            AddEntry(archive, "OEBPS/toc.ncx", TocNcxXml);

            // chapter1.xhtml
            AddEntry(archive, "OEBPS/Text/chapter1.xhtml", Chapter1Xhtml);

            // cover.jpg
            AddEntry(archive, "OEBPS/Images/cover.jpg", CreateMinimalJpeg());
        }

        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// 创建包含自定义元数据的 EPUB。
    /// </summary>
    public static MemoryStream CreateEpubWithCustomMetadata()
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "mimetype", "application/epub+zip");
            AddEntry(archive, "META-INF/container.xml", ContainerXml);
            AddEntry(archive, "OEBPS/content.opf", OpfWithCustomMetadata);
            AddEntry(archive, "OEBPS/nav.xhtml", NavXhtml);
            AddEntry(archive, "OEBPS/Text/chapter1.xhtml", Chapter1Xhtml);
        }

        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// 创建包含多层嵌套目录的 EPUB。
    /// </summary>
    public static MemoryStream CreateEpubWithNestedToc()
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "mimetype", "application/epub+zip");
            AddEntry(archive, "META-INF/container.xml", ContainerXml);
            AddEntry(archive, "OEBPS/content.opf", OpfWithMultipleChapters);
            AddEntry(archive, "OEBPS/nav.xhtml", NestedNavXhtml);
            AddEntry(archive, "OEBPS/Text/chapter1.xhtml", Chapter1Xhtml);
            AddEntry(archive, "OEBPS/Text/chapter2.xhtml", Chapter2Xhtml);
            AddEntry(archive, "OEBPS/Text/chapter3.xhtml", Chapter3Xhtml);
        }

        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// 创建漫画/图片为主的 EPUB（固定布局）。
    /// </summary>
    public static MemoryStream CreateMangaEpub()
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "mimetype", "application/epub+zip");
            AddEntry(archive, "META-INF/container.xml", ContainerXml);
            AddEntry(archive, "OEBPS/content.opf", MangaOpfXml);
            AddEntry(archive, "OEBPS/nav.xhtml", NavXhtml);
            AddEntry(archive, "OEBPS/Images/page001.jpg", CreateMinimalJpeg());
            AddEntry(archive, "OEBPS/Images/page002.jpg", CreateMinimalJpeg());
            AddEntry(archive, "OEBPS/Images/cover.jpg", CreateMinimalJpeg());
            AddEntry(archive, "OEBPS/Text/page1.xhtml", ImagePageXhtml("../Images/page001.jpg"));
            AddEntry(archive, "OEBPS/Text/page2.xhtml", ImagePageXhtml("../Images/page002.jpg"));
        }

        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// 创建 EPUB 2 格式的 EPUB（使用 NCX 导航）。
    /// </summary>
    public static MemoryStream CreateEpub2Stream()
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "mimetype", "application/epub+zip");
            AddEntry(archive, "META-INF/container.xml", ContainerXml);
            AddEntry(archive, "OEBPS/content.opf", Epub2OpfXml);
            AddEntry(archive, "OEBPS/toc.ncx", TocNcxXml);
            AddEntry(archive, "OEBPS/Text/chapter1.xhtml", Chapter1Xhtml);
            AddEntry(archive, "OEBPS/Images/cover.jpg", CreateMinimalJpeg());
        }

        stream.Position = 0;
        return stream;
    }

    private static void AddEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(content);
    }

    private static void AddEntry(ZipArchive archive, string path, byte[] content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
        using var stream = entry.Open();
        stream.Write(content, 0, content.Length);
    }

    /// <summary>
    /// 创建最小的有效 JPEG 图片（1x1 像素）。
    /// </summary>
    public static byte[] CreateMinimalJpeg()
    {
        return
        [
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
            0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
            0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07, 0x07, 0x07, 0x09,
            0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
            0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20,
            0x24, 0x2E, 0x27, 0x20, 0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
            0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39, 0x3D, 0x38, 0x32,
            0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01,
            0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x1F, 0x00, 0x00,
            0x01, 0x05, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0xFF, 0xC4, 0x00, 0xB5, 0x10, 0x00, 0x02, 0x01, 0x03,
            0x03, 0x02, 0x04, 0x03, 0x05, 0x05, 0x04, 0x04, 0x00, 0x00, 0x01, 0x7D,
            0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21, 0x31, 0x41, 0x06,
            0x13, 0x51, 0x61, 0x07, 0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xA1, 0x08,
            0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3F, 0x00, 0x7F, 0xFF,
            0xD9
        ];
    }

    /// <summary>
    /// 创建最小的有效 PNG 图片（1x1 像素）。
    /// </summary>
    public static byte[] CreateMinimalPng()
    {
        return
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89, 0x00, 0x00, 0x00,
            0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
            0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x49,
            0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
        ];
    }

    #region XML Templates

    private const string ContainerXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
            <rootfiles>
                <rootfile full-path="OEBPS/content.opf" media-type="application/oebps-package+xml"/>
            </rootfiles>
        </container>
        """;

    private const string MinimalOpfXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <package xmlns="http://www.idpf.org/2007/opf" version="3.0" unique-identifier="uid">
            <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:identifier id="uid">urn:uuid:12345678-1234-1234-1234-123456789012</dc:identifier>
                <dc:title>测试书籍</dc:title>
                <dc:creator>测试作者</dc:creator>
                <dc:language>zh</dc:language>
                <dc:publisher>测试出版社</dc:publisher>
                <dc:description>这是一本测试书籍</dc:description>
                <meta property="dcterms:modified">2024-01-01T00:00:00Z</meta>
            </metadata>
            <manifest>
                <item id="nav" href="nav.xhtml" media-type="application/xhtml+xml" properties="nav"/>
                <item id="ncx" href="toc.ncx" media-type="application/x-dtbncx+xml"/>
                <item id="chapter1" href="Text/chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="cover-image" href="Images/cover.jpg" media-type="image/jpeg" properties="cover-image"/>
            </manifest>
            <spine toc="ncx">
                <itemref idref="chapter1"/>
            </spine>
        </package>
        """;

    private const string OpfWithCustomMetadata = """
        <?xml version="1.0" encoding="UTF-8"?>
        <package xmlns="http://www.idpf.org/2007/opf" version="3.0" unique-identifier="uid">
            <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:identifier id="uid">urn:uuid:custom-meta-test</dc:identifier>
                <dc:title>自定义元数据测试</dc:title>
                <dc:creator>作者A</dc:creator>
                <dc:contributor>贡献者B</dc:contributor>
                <dc:contributor>贡献者C</dc:contributor>
                <dc:language>zh</dc:language>
                <dc:subject>科幻</dc:subject>
                <dc:subject>冒险</dc:subject>
                <meta property="dcterms:modified">2024-06-15T10:30:00Z</meta>
                <meta name="generator" content="EpubParser.Test"/>
                <meta name="source" content="https://example.com"/>
                <meta property="rendition:layout">reflowable</meta>
                <meta property="rendition:orientation">auto</meta>
            </metadata>
            <manifest>
                <item id="nav" href="nav.xhtml" media-type="application/xhtml+xml" properties="nav"/>
                <item id="chapter1" href="Text/chapter1.xhtml" media-type="application/xhtml+xml"/>
            </manifest>
            <spine>
                <itemref idref="chapter1"/>
            </spine>
        </package>
        """;

    private const string OpfWithMultipleChapters = """
        <?xml version="1.0" encoding="UTF-8"?>
        <package xmlns="http://www.idpf.org/2007/opf" version="3.0" unique-identifier="uid">
            <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:identifier id="uid">urn:uuid:multi-chapter</dc:identifier>
                <dc:title>多章节测试</dc:title>
                <dc:creator>测试作者</dc:creator>
                <dc:language>zh</dc:language>
            </metadata>
            <manifest>
                <item id="nav" href="nav.xhtml" media-type="application/xhtml+xml" properties="nav"/>
                <item id="chapter1" href="Text/chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="chapter2" href="Text/chapter2.xhtml" media-type="application/xhtml+xml"/>
                <item id="chapter3" href="Text/chapter3.xhtml" media-type="application/xhtml+xml"/>
            </manifest>
            <spine>
                <itemref idref="chapter1"/>
                <itemref idref="chapter2"/>
                <itemref idref="chapter3"/>
            </spine>
        </package>
        """;

    private const string MangaOpfXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <package xmlns="http://www.idpf.org/2007/opf" version="3.0" unique-identifier="uid" dir="rtl">
            <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:identifier id="uid">urn:uuid:manga-test</dc:identifier>
                <dc:title>漫画测试</dc:title>
                <dc:creator>漫画家</dc:creator>
                <dc:language>ja</dc:language>
                <meta property="rendition:layout">pre-paginated</meta>
                <meta property="rendition:spread">landscape</meta>
            </metadata>
            <manifest>
                <item id="nav" href="nav.xhtml" media-type="application/xhtml+xml" properties="nav"/>
                <item id="cover" href="Images/cover.jpg" media-type="image/jpeg" properties="cover-image"/>
                <item id="page1" href="Text/page1.xhtml" media-type="application/xhtml+xml"/>
                <item id="page2" href="Text/page2.xhtml" media-type="application/xhtml+xml"/>
                <item id="img1" href="Images/page001.jpg" media-type="image/jpeg"/>
                <item id="img2" href="Images/page002.jpg" media-type="image/jpeg"/>
            </manifest>
            <spine page-progression-direction="rtl">
                <itemref idref="page1"/>
                <itemref idref="page2"/>
            </spine>
        </package>
        """;

    private const string Epub2OpfXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <package xmlns="http://www.idpf.org/2007/opf" version="2.0" unique-identifier="uid">
            <metadata xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:opf="http://www.idpf.org/2007/opf">
                <dc:identifier id="uid">urn:uuid:epub2-test</dc:identifier>
                <dc:title>EPUB2 测试</dc:title>
                <dc:creator opf:role="aut">EPUB2 作者</dc:creator>
                <dc:language>zh</dc:language>
                <meta name="cover" content="cover-image"/>
            </metadata>
            <manifest>
                <item id="ncx" href="toc.ncx" media-type="application/x-dtbncx+xml"/>
                <item id="chapter1" href="Text/chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="cover-image" href="Images/cover.jpg" media-type="image/jpeg"/>
            </manifest>
            <spine toc="ncx">
                <itemref idref="chapter1"/>
            </spine>
            <guide>
                <reference type="cover" href="Images/cover.jpg" title="Cover"/>
            </guide>
        </package>
        """;

    private const string NavXhtml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops">
        <head>
            <title>目录</title>
        </head>
        <body>
            <nav epub:type="toc">
                <h1>目录</h1>
                <ol>
                    <li><a href="Text/chapter1.xhtml">第一章</a></li>
                </ol>
            </nav>
        </body>
        </html>
        """;

    private const string NestedNavXhtml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops">
        <head>
            <title>目录</title>
        </head>
        <body>
            <nav epub:type="toc">
                <h1>目录</h1>
                <ol>
                    <li>
                        <a href="Text/chapter1.xhtml">第一部分</a>
                        <ol>
                            <li><a href="Text/chapter2.xhtml">第一章</a></li>
                            <li><a href="Text/chapter3.xhtml">第二章</a></li>
                        </ol>
                    </li>
                </ol>
            </nav>
        </body>
        </html>
        """;

    private const string TocNcxXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <ncx xmlns="http://www.daisy.org/z3986/2005/ncx/" version="2005-1">
            <head>
                <meta name="dtb:uid" content="urn:uuid:12345678-1234-1234-1234-123456789012"/>
            </head>
            <docTitle>
                <text>测试书籍</text>
            </docTitle>
            <navMap>
                <navPoint id="navpoint-1" playOrder="1">
                    <navLabel>
                        <text>第一章</text>
                    </navLabel>
                    <content src="Text/chapter1.xhtml"/>
                </navPoint>
            </navMap>
        </ncx>
        """;

    private const string Chapter1Xhtml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml">
        <head>
            <title>第一章</title>
        </head>
        <body>
            <h1>第一章</h1>
            <p>这是第一章的内容。</p>
        </body>
        </html>
        """;

    private const string Chapter2Xhtml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml">
        <head>
            <title>第一章</title>
        </head>
        <body>
            <h1>第一章</h1>
            <p>这是第一章的内容。</p>
        </body>
        </html>
        """;

    private const string Chapter3Xhtml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml">
        <head>
            <title>第二章</title>
        </head>
        <body>
            <h1>第二章</h1>
            <p>这是第二章的内容。</p>
        </body>
        </html>
        """;

    private static string ImagePageXhtml(string imagePath) => $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml">
        <head>
            <title>Page</title>
        </head>
        <body>
            <img src="{imagePath}" alt="page"/>
        </body>
        </html>
        """;

    #endregion
}

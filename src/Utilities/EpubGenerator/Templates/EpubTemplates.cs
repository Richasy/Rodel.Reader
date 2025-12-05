// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// EPUB 模板常量.
/// </summary>
internal static class EpubTemplates
{
    /// <summary>
    /// MIME 类型.
    /// </summary>
    public const string Mimetype = "application/epub+zip";

    /// <summary>
    /// container.xml 模板.
    /// </summary>
    public const string Container = """
        <?xml version="1.0" encoding="UTF-8"?>
        <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
            <rootfiles>
                <rootfile full-path="OEBPS/content.opf" media-type="application/oebps-package+xml"/>
            </rootfiles>
        </container>
        """;

    /// <summary>
    /// content.opf 模板 (EPUB 2).
    /// </summary>
    public const string ContentOpfEpub2 = """
        <?xml version="1.0" encoding="UTF-8"?>
        <package xmlns="http://www.idpf.org/2007/opf" version="2.0" unique-identifier="BookId">
            <metadata xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:opf="http://www.idpf.org/2007/opf">
                <dc:title>{{Title}}</dc:title>
                <dc:creator opf:role="aut">{{Author}}</dc:creator>
                <dc:language>{{Language}}</dc:language>
                <dc:identifier id="BookId" opf:scheme="UUID">{{Identifier}}</dc:identifier>
                <dc:date>{{Date}}</dc:date>
        {{Description}}
        {{Publisher}}
        {{Subjects}}
        {{Contributors}}
        {{CoverMeta}}
        {{CustomMetadata}}
            </metadata>
            <manifest>
                <item id="ncx" href="toc.ncx" media-type="application/x-dtbncx+xml"/>
                <item id="style" href="Styles/main.css" media-type="text/css"/>
        {{CoverImageItem}}
        {{CoverPageItem}}
                <item id="titlepage" href="Text/titlepage.xhtml" media-type="application/xhtml+xml"/>
        {{TocPageItem}}
        {{CopyrightPageItem}}
        {{ChapterItems}}
        {{ChapterImageItems}}
        {{ResourceItems}}
            </manifest>
            <spine toc="ncx">
        {{CoverPageRef}}
                <itemref idref="titlepage"/>
        {{TocPageRef}}
        {{CopyrightPageRef}}
        {{ChapterRefs}}
            </spine>
            <guide>
        {{CoverGuide}}
                <reference type="title-page" title="Title Page" href="Text/titlepage.xhtml"/>
        {{TocGuide}}
            </guide>
        </package>
        """;

    /// <summary>
    /// content.opf 模板 (EPUB 3).
    /// </summary>
    public const string ContentOpfEpub3 = """
        <?xml version="1.0" encoding="UTF-8"?>
        <package xmlns="http://www.idpf.org/2007/opf" version="3.0" unique-identifier="BookId" xml:lang="{{Language}}" dir="{{Direction}}">
            <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>{{Title}}</dc:title>
                <dc:creator>{{Author}}</dc:creator>
                <dc:language>{{Language}}</dc:language>
                <dc:identifier id="BookId">{{Identifier}}</dc:identifier>
                <meta property="dcterms:modified">{{ModifiedDate}}</meta>
        {{Description}}
        {{Publisher}}
        {{Subjects}}
        {{Contributors}}
        {{CoverMeta}}
        {{CustomMetadata}}
            </metadata>
            <manifest>
                <item id="ncx" href="toc.ncx" media-type="application/x-dtbncx+xml"/>
                <item id="nav" href="nav.xhtml" media-type="application/xhtml+xml" properties="nav"/>
                <item id="style" href="Styles/main.css" media-type="text/css"/>
        {{CoverImageItem}}
        {{CoverPageItem}}
                <item id="titlepage" href="Text/titlepage.xhtml" media-type="application/xhtml+xml"/>
        {{TocPageItem}}
        {{CopyrightPageItem}}
        {{ChapterItems}}
        {{ChapterImageItems}}
        {{ResourceItems}}
            </manifest>
            <spine page-progression-direction="{{PageProgression}}">
        {{CoverPageRef}}
                <itemref idref="titlepage"/>
        {{TocPageRef}}
        {{CopyrightPageRef}}
        {{ChapterRefs}}
            </spine>
        </package>
        """;

    /// <summary>
    /// toc.ncx 模板.
    /// </summary>
    public const string TocNcx = """
        <?xml version="1.0" encoding="UTF-8"?>
        <ncx xmlns="http://www.daisy.org/z3986/2005/ncx/" version="2005-1">
            <head>
                <meta name="dtb:uid" content="{{Identifier}}"/>
                <meta name="dtb:depth" content="1"/>
                <meta name="dtb:totalPageCount" content="0"/>
                <meta name="dtb:maxPageNumber" content="0"/>
            </head>
            <docTitle>
                <text>{{Title}}</text>
            </docTitle>
            <docAuthor>
                <text>{{Author}}</text>
            </docAuthor>
            <navMap>
        {{NavPoints}}
            </navMap>
        </ncx>
        """;

    /// <summary>
    /// nav.xhtml 模板 (EPUB 3).
    /// </summary>
    public const string NavDoc = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="{{Language}}">
        <head>
            <meta charset="UTF-8"/>
            <title>{{Title}}</title>
            <link rel="stylesheet" type="text/css" href="Styles/main.css"/>
        </head>
        <body>
            <nav epub:type="toc" id="toc">
                <h1>目录</h1>
                <ol>
        {{NavItems}}
                </ol>
            </nav>
        </body>
        </html>
        """;

    /// <summary>
    /// 封面页模板.
    /// </summary>
    public const string CoverPage = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="{{Language}}">
        <head>
            <meta charset="UTF-8"/>
            <title>{{Title}}</title>
            <style type="text/css">
                body { margin: 0; padding: 0; text-align: center; }
                .cover { width: 100%; height: 100%; }
                .cover img { max-width: 100%; max-height: 100%; object-fit: contain; }
            </style>
        </head>
        <body>
            <div class="cover">
                <img src="../Images/{{CoverFileName}}" alt="{{Title}}"/>
            </div>
        </body>
        </html>
        """;

    /// <summary>
    /// 标题页模板.
    /// </summary>
    public const string TitlePage = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="{{Language}}">
        <head>
            <meta charset="UTF-8"/>
            <title>{{Title}}</title>
            <link rel="stylesheet" type="text/css" href="../Styles/main.css"/>
        </head>
        <body class="titlepage">
            <div class="title-container">
                <h1 class="book-title">{{Title}}</h1>
        {{AuthorSection}}
            </div>
        </body>
        </html>
        """;

    /// <summary>
    /// 目录页模板.
    /// </summary>
    public const string TocPage = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="{{Language}}">
        <head>
            <meta charset="UTF-8"/>
            <title>{{TocTitle}}</title>
            <link rel="stylesheet" type="text/css" href="../Styles/main.css"/>
        </head>
        <body class="tocpage">
            <div class="toc-container">
                <h1 class="toc-title">{{TocTitle}}</h1>
                <nav class="toc-nav">
                    <ol class="toc-list">
        {{TocItems}}
                    </ol>
                </nav>
            </div>
        </body>
        </html>
        """;

    /// <summary>
    /// 版权页模板.
    /// </summary>
    public const string CopyrightPage = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="{{Language}}">
        <head>
            <meta charset="UTF-8"/>
            <title>版权信息</title>
            <link rel="stylesheet" type="text/css" href="../Styles/main.css"/>
        </head>
        <body class="copyrightpage">
            <div class="copyright-container">
                <h1 class="book-title">{{Title}}</h1>
        {{AuthorSection}}
        {{PublisherSection}}
        {{IsbnSection}}
        {{EditionSection}}
        {{PublishDateSection}}
        {{CopyrightSection}}
        {{RightsSection}}
            </div>
        </body>
        </html>
        """;

    /// <summary>
    /// 章节页模板.
    /// </summary>
    public const string ChapterPage = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="{{Language}}">
        <head>
            <meta charset="UTF-8"/>
            <title>{{Title}}</title>
            <link rel="stylesheet" type="text/css" href="../Styles/main.css"/>
        </head>
        <body class="chapter">
            <div class="chapter-container">
                <h1 class="chapter-title">{{Title}}</h1>
                <div class="chapter-content">
        {{Content}}
                </div>
            </div>
        </body>
        </html>
        """;

    /// <summary>
    /// 默认样式表.
    /// </summary>
    /// <remarks>
    /// 注意：不设置 font-family、color、background-color，让阅读器可以覆写这些属性.
    /// </remarks>
    public const string DefaultStyleSheet = """
        /* EPUB Default Stylesheet */
        /* 不设置字体、颜色、背景色，让阅读器可以自由覆写 */
        
        /* Body */
        body {
            font-size: 1em;
            line-height: 1.6;
            padding: 1em;
        }
        
        /* Title Page */
        .titlepage {
            text-align: center;
            padding-top: 30%;
        }
        
        .title-container {
            margin: 0 auto;
        }
        
        .book-title {
            font-size: 2em;
            font-weight: bold;
            margin-bottom: 1em;
            line-height: 1.2;
        }
        
        .book-author {
            font-size: 1.2em;
            opacity: 0.7;
        }
        
        /* TOC Page */
        .tocpage {
            padding: 2em 1em;
        }
        
        .toc-title {
            font-size: 1.5em;
            text-align: center;
            margin-bottom: 1.5em;
            border-bottom: 1px solid currentColor;
            padding-bottom: 0.5em;
            opacity: 0.3;
        }
        
        .toc-list {
            list-style: none;
        }
        
        .toc-list li {
            margin: 0.5em 0;
        }
        
        .toc-list a {
            text-decoration: none;
        }
        
        .toc-list a:hover {
            text-decoration: underline;
        }
        
        /* Copyright Page */
        .copyrightpage {
            padding: 2em 1em;
            font-size: 0.9em;
        }
        
        .copyright-container {
            max-width: 80%;
            margin: 0 auto;
        }
        
        .copyright-container p {
            margin: 0.5em 0;
        }
        
        .copyright-container .book-title {
            font-size: 1.3em;
            margin-bottom: 1.5em;
        }
        
        /* Chapter */
        .chapter {
            padding: 1em;
        }
        
        .chapter-title {
            font-size: 1.5em;
            text-align: center;
            margin-bottom: 1.5em;
            line-height: 1.3;
        }
        
        .chapter-content {
            text-align: justify;
        }
        
        .chapter-content p {
            text-indent: 2em;
            margin: 0.8em 0;
        }
        
        /* Links */
        a {
            text-decoration: none;
        }
        
        a:hover {
            text-decoration: underline;
        }
        
        /* Images */
        img {
            max-width: 100%;
            height: auto;
        }
        
        .image-container {
            text-align: center;
            margin: 1.5em 0;
        }
        
        .image-container img {
            max-width: 100%;
            height: auto;
        }
        
        .image-figure {
            text-align: center;
            margin: 1.5em 0;
        }
        
        .image-figure img {
            max-width: 100%;
            height: auto;
        }
        
        .image-figure figcaption {
            font-size: 0.9em;
            opacity: 0.7;
            margin-top: 0.5em;
            font-style: italic;
        }
        
        /* Navigation (EPUB 3) */
        nav[epub|type="toc"] {
            padding: 1em;
        }
        
        nav[epub|type="toc"] h1 {
            font-size: 1.5em;
            margin-bottom: 1em;
        }
        
        nav[epub|type="toc"] ol {
            list-style: none;
        }
        
        nav[epub|type="toc"] li {
            margin: 0.5em 0;
        }
        """;
}

// Copyright (c) Richasy. All rights reserved.

using System.Text;

namespace Fb2Parser.Test;

/// <summary>
/// 测试数据工厂。
/// </summary>
internal static class TestDataFactory
{
    /// <summary>
    /// 创建最小的有效 FB2 内容。
    /// </summary>
    public static string CreateMinimalFb2()
    {
        return """
            <?xml version="1.0" encoding="utf-8"?>
            <FictionBook xmlns="http://www.gribuser.ru/xml/fictionbook/2.0" xmlns:l="http://www.w3.org/1999/xlink">
                <description>
                    <title-info>
                        <author>
                            <first-name>John</first-name>
                            <last-name>Doe</last-name>
                        </author>
                        <book-title>Test Book</book-title>
                        <lang>en</lang>
                    </title-info>
                </description>
                <body>
                    <section>
                        <title><p>Chapter 1</p></title>
                        <p>This is the first paragraph.</p>
                    </section>
                </body>
            </FictionBook>
            """;
    }

    /// <summary>
    /// 创建包含完整元数据的 FB2 内容。
    /// </summary>
    public static string CreateFb2WithFullMetadata()
    {
        return """
            <?xml version="1.0" encoding="utf-8"?>
            <FictionBook xmlns="http://www.gribuser.ru/xml/fictionbook/2.0" xmlns:l="http://www.w3.org/1999/xlink">
                <description>
                    <title-info>
                        <genre>science_fiction</genre>
                        <genre>adventure</genre>
                        <author>
                            <first-name>John</first-name>
                            <middle-name>William</middle-name>
                            <last-name>Doe</last-name>
                            <nickname>JohnnyD</nickname>
                            <home-page>https://example.com</home-page>
                            <email>john@example.com</email>
                        </author>
                        <author>
                            <first-name>Jane</first-name>
                            <last-name>Smith</last-name>
                        </author>
                        <book-title>The Great Adventure</book-title>
                        <annotation>
                            <p>This is a great book about adventures.</p>
                            <p>It has many exciting chapters.</p>
                        </annotation>
                        <keywords>adventure, science fiction, space</keywords>
                        <date value="2023-01-15">January 15, 2023</date>
                        <lang>en</lang>
                        <src-lang>ru</src-lang>
                        <translator>
                            <first-name>Alex</first-name>
                            <last-name>Translator</last-name>
                        </translator>
                        <sequence name="Space Saga" number="1"/>
                    </title-info>
                    <document-info>
                        <author>
                            <nickname>converter</nickname>
                        </author>
                        <program-used>FB2 Creator</program-used>
                        <date value="2023-02-01">February 2023</date>
                        <src-url>https://source.example.com</src-url>
                        <id>unique-doc-id-12345</id>
                        <version>1.0</version>
                        <history>
                            <p>First version created.</p>
                        </history>
                    </document-info>
                    <publish-info>
                        <book-name>The Great Adventure: Paperback Edition</book-name>
                        <publisher>Amazing Books Publishing</publisher>
                        <city>New York</city>
                        <year>2023</year>
                        <isbn>978-1234567890</isbn>
                        <sequence name="Bestsellers" number="5"/>
                    </publish-info>
                </description>
                <body>
                    <section>
                        <title><p>Chapter 1</p></title>
                        <p>Content of chapter 1.</p>
                    </section>
                </body>
            </FictionBook>
            """;
    }

    /// <summary>
    /// 创建包含嵌套章节的 FB2 内容。
    /// </summary>
    public static string CreateFb2WithNestedSections()
    {
        return """
            <?xml version="1.0" encoding="utf-8"?>
            <FictionBook xmlns="http://www.gribuser.ru/xml/fictionbook/2.0" xmlns:l="http://www.w3.org/1999/xlink">
                <description>
                    <title-info>
                        <author><first-name>Author</first-name><last-name>Name</last-name></author>
                        <book-title>Nested Sections Book</book-title>
                        <lang>en</lang>
                    </title-info>
                </description>
                <body>
                    <section id="part1">
                        <title><p>Part 1</p></title>
                        <p>Introduction to Part 1.</p>
                        <section id="chapter1">
                            <title><p>Chapter 1</p></title>
                            <p>Content of Chapter 1.</p>
                            <section id="section1-1">
                                <title><p>Section 1.1</p></title>
                                <p>Content of Section 1.1.</p>
                            </section>
                            <section id="section1-2">
                                <title><p>Section 1.2</p></title>
                                <p>Content of Section 1.2.</p>
                            </section>
                        </section>
                        <section id="chapter2">
                            <title><p>Chapter 2</p></title>
                            <p>Content of Chapter 2.</p>
                        </section>
                    </section>
                    <section id="part2">
                        <title><p>Part 2</p></title>
                        <p>Introduction to Part 2.</p>
                    </section>
                </body>
            </FictionBook>
            """;
    }

    /// <summary>
    /// 创建包含封面和图片的 FB2 内容。
    /// </summary>
    public static string CreateFb2WithCoverAndImages()
    {
        // 创建最小的有效 JPEG 数据（1x1 像素红色图片）
        var jpegBytes = CreateMinimalJpeg();
        var jpegBase64 = Convert.ToBase64String(jpegBytes);

        // 创建另一张图片
        var pngBytes = CreateMinimalPng();
        var pngBase64 = Convert.ToBase64String(pngBytes);

        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <FictionBook xmlns="http://www.gribuser.ru/xml/fictionbook/2.0" xmlns:l="http://www.w3.org/1999/xlink">
                <description>
                    <title-info>
                        <author><first-name>Author</first-name><last-name>Name</last-name></author>
                        <book-title>Book With Images</book-title>
                        <coverpage>
                            <image l:href="#cover.jpg"/>
                        </coverpage>
                        <lang>en</lang>
                    </title-info>
                </description>
                <body>
                    <section>
                        <title><p>Chapter 1</p></title>
                        <p>Here is an image:</p>
                        <image l:href="#image1.png"/>
                        <p>End of chapter.</p>
                    </section>
                </body>
                <binary id="cover.jpg" content-type="image/jpeg">{jpegBase64}</binary>
                <binary id="image1.png" content-type="image/png">{pngBase64}</binary>
            </FictionBook>
            """;
    }

    /// <summary>
    /// 创建包含诗歌和引用的 FB2 内容。
    /// </summary>
    public static string CreateFb2WithPoemAndCite()
    {
        return """
            <?xml version="1.0" encoding="utf-8"?>
            <FictionBook xmlns="http://www.gribuser.ru/xml/fictionbook/2.0" xmlns:l="http://www.w3.org/1999/xlink">
                <description>
                    <title-info>
                        <author><first-name>Poet</first-name><last-name>Name</last-name></author>
                        <book-title>Poetry Collection</book-title>
                        <lang>en</lang>
                    </title-info>
                </description>
                <body>
                    <section>
                        <title><p>Poems</p></title>
                        <poem>
                            <title><p>My Poem</p></title>
                            <stanza>
                                <v>First line of the poem,</v>
                                <v>Second line with rhyme.</v>
                            </stanza>
                            <stanza>
                                <v>Third line continues,</v>
                                <v>Fourth line on time.</v>
                            </stanza>
                            <text-author>Poet Name</text-author>
                        </poem>
                        <cite>
                            <p>This is a famous quote.</p>
                            <text-author>Famous Author</text-author>
                        </cite>
                    </section>
                </body>
            </FictionBook>
            """;
    }

    /// <summary>
    /// 创建无命名空间的 FB2 内容（兼容性测试）。
    /// </summary>
    public static string CreateFb2WithoutNamespace()
    {
        return """
            <?xml version="1.0" encoding="utf-8"?>
            <FictionBook>
                <description>
                    <title-info>
                        <author>
                            <first-name>Author</first-name>
                            <last-name>Name</last-name>
                        </author>
                        <book-title>Book Without Namespace</book-title>
                        <lang>en</lang>
                    </title-info>
                </description>
                <body>
                    <section>
                        <title><p>Chapter 1</p></title>
                        <p>Content without namespace.</p>
                    </section>
                </body>
            </FictionBook>
            """;
    }

    /// <summary>
    /// 创建空 FB2 内容（最小化）。
    /// </summary>
    public static string CreateEmptyFb2()
    {
        return """
            <?xml version="1.0" encoding="utf-8"?>
            <FictionBook xmlns="http://www.gribuser.ru/xml/fictionbook/2.0">
                <description>
                    <title-info>
                        <book-title>Empty Book</book-title>
                    </title-info>
                </description>
                <body>
                </body>
            </FictionBook>
            """;
    }

    /// <summary>
    /// 创建多 body 的 FB2 内容（包含注释）。
    /// </summary>
    public static string CreateFb2WithMultipleBodies()
    {
        return """
            <?xml version="1.0" encoding="utf-8"?>
            <FictionBook xmlns="http://www.gribuser.ru/xml/fictionbook/2.0" xmlns:l="http://www.w3.org/1999/xlink">
                <description>
                    <title-info>
                        <author><first-name>Author</first-name><last-name>Name</last-name></author>
                        <book-title>Book With Notes</book-title>
                        <lang>en</lang>
                    </title-info>
                </description>
                <body>
                    <section id="chapter1">
                        <title><p>Chapter 1</p></title>
                        <p>Main content with a note reference.</p>
                    </section>
                </body>
                <body name="notes">
                    <section id="note1">
                        <title><p>Note 1</p></title>
                        <p>This is a footnote.</p>
                    </section>
                </body>
            </FictionBook>
            """;
    }

    /// <summary>
    /// 创建包含特殊编码的 FB2 内容。
    /// </summary>
    public static string CreateFb2WithSpecialCharacters()
    {
        return """
            <?xml version="1.0" encoding="utf-8"?>
            <FictionBook xmlns="http://www.gribuser.ru/xml/fictionbook/2.0">
                <description>
                    <title-info>
                        <author>
                            <first-name>Иван</first-name>
                            <last-name>Петров</last-name>
                        </author>
                        <book-title>Книга с кириллицей</book-title>
                        <annotation>
                            <p>Описание книги с особыми символами: &lt; &gt; &amp; "цитата"</p>
                        </annotation>
                        <lang>ru</lang>
                    </title-info>
                </description>
                <body>
                    <section>
                        <title><p>Глава 1</p></title>
                        <p>Текст с кириллицей и специальными символами: © ® ™ € £ ¥</p>
                    </section>
                </body>
            </FictionBook>
            """;
    }

    /// <summary>
    /// 创建带有作者昵称的 FB2 内容。
    /// </summary>
    public static string CreateFb2WithNicknameAuthor()
    {
        return """
            <?xml version="1.0" encoding="utf-8"?>
            <FictionBook xmlns="http://www.gribuser.ru/xml/fictionbook/2.0">
                <description>
                    <title-info>
                        <author>
                            <nickname>PenName42</nickname>
                        </author>
                        <book-title>Anonymous Book</book-title>
                        <lang>en</lang>
                    </title-info>
                </description>
                <body>
                    <section>
                        <title><p>Chapter 1</p></title>
                        <p>Written by a nickname author.</p>
                    </section>
                </body>
            </FictionBook>
            """;
    }

    /// <summary>
    /// 创建最小的有效 JPEG 图片（1x1 像素）。
    /// </summary>
    public static byte[] CreateMinimalJpeg()
    {
        // 最小的有效 JPEG：1x1 红色像素
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
            0x23, 0x42, 0xB1, 0xC1, 0x15, 0x52, 0xD1, 0xF0, 0x24, 0x33, 0x62, 0x72,
            0x82, 0x09, 0x0A, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x25, 0x26, 0x27, 0x28,
            0x29, 0x2A, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x43, 0x44, 0x45,
            0x46, 0x47, 0x48, 0x49, 0x4A, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59,
            0x5A, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x73, 0x74, 0x75,
            0x76, 0x77, 0x78, 0x79, 0x7A, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89,
            0x8A, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0xA2, 0xA3,
            0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6,
            0xB7, 0xB8, 0xB9, 0xBA, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9,
            0xCA, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xE1, 0xE2,
            0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xF1, 0xF2, 0xF3, 0xF4,
            0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01,
            0x00, 0x00, 0x3F, 0x00, 0xFB, 0xD5, 0xDB, 0x20, 0xA8, 0xF1, 0x7E, 0xCD,
            0xBF, 0xFF, 0xD9
        ];
    }

    /// <summary>
    /// 创建最小的有效 PNG 图片（1x1 像素）。
    /// </summary>
    public static byte[] CreateMinimalPng()
    {
        // 最小的有效 PNG：1x1 红色像素
        return
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, // IHDR length
            0x49, 0x48, 0x44, 0x52, // "IHDR"
            0x00, 0x00, 0x00, 0x01, // width: 1
            0x00, 0x00, 0x00, 0x01, // height: 1
            0x08, // bit depth: 8
            0x02, // color type: RGB
            0x00, // compression: deflate
            0x00, // filter: adaptive
            0x00, // interlace: none
            0x90, 0x77, 0x53, 0xDE, // CRC
            0x00, 0x00, 0x00, 0x0C, // IDAT length
            0x49, 0x44, 0x41, 0x54, // "IDAT"
            0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00, 0x00, // compressed data
            0x01, 0x01, 0x01, 0x00, // CRC (approximate)
            0xE6, 0xAE, 0xB2, 0xA5, // CRC
            0x00, 0x00, 0x00, 0x00, // IEND length
            0x49, 0x45, 0x4E, 0x44, // "IEND"
            0xAE, 0x42, 0x60, 0x82  // CRC
        ];
    }

    /// <summary>
    /// 将内容保存为临时文件。
    /// </summary>
    public static string SaveToTempFile(string content, string extension = ".fb2")
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"fb2test_{Guid.NewGuid()}{extension}");
        File.WriteAllText(tempPath, content, Encoding.UTF8);
        return tempPath;
    }

    /// <summary>
    /// 将内容保存为临时流。
    /// </summary>
    public static MemoryStream CreateStreamFromContent(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }
}

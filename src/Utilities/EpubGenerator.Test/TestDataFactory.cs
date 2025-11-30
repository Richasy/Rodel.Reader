// Copyright (c) Reader Copilot. All rights reserved.

namespace EpubGenerator.Test;

/// <summary>
/// 测试数据工厂.
/// </summary>
internal static class TestDataFactory
{
    public static EpubMetadata CreateBasicMetadata(string title = "测试书籍", string? author = "测试作者")
    {
        return new EpubMetadata
        {
            Title = title,
            Author = author,
            Language = "zh",
            Identifier = "test-uuid-12345",
            Description = "这是一本测试书籍",
            Publisher = "测试出版社",
            PublishDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
        };
    }

    public static EpubMetadata CreateMetadataWithCover()
    {
        var metadata = CreateBasicMetadata();
        return new EpubMetadata
        {
            Title = metadata.Title,
            Author = metadata.Author,
            Language = metadata.Language,
            Identifier = metadata.Identifier,
            Cover = CreateCoverInfo(),
        };
    }

    public static EpubMetadata CreateMetadataWithCopyright()
    {
        var metadata = CreateBasicMetadata();
        return new EpubMetadata
        {
            Title = metadata.Title,
            Author = metadata.Author,
            Language = metadata.Language,
            Identifier = metadata.Identifier,
            Copyright = CreateCopyrightInfo(),
        };
    }

    public static EpubMetadata CreateFullMetadata()
    {
        return new EpubMetadata
        {
            Title = "完整测试书籍",
            Author = "测试作者",
            Language = "zh",
            Identifier = "test-uuid-full",
            Description = "完整的测试书籍描述",
            Publisher = "测试出版社",
            PublishDate = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Cover = CreateCoverInfo(),
            Copyright = CreateCopyrightInfo(),
            Subjects = ["小说", "科幻", "冒险"],
            Contributors = ["编辑A", "译者B"],
            CustomMetadata =
            [
                CustomMetadata.Create("generator", "EpubGenerator.Test"),
                CustomMetadata.Create("source", "https://example.com"),
            ],
        };
    }

    public static CoverInfo CreateCoverInfo()
    {
        // 创建一个最小的有效 JPEG 图片数据（1x1 像素）
        byte[] minimalJpeg =
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

        return CoverInfo.FromBytes(minimalJpeg, "image/jpeg");
    }

    public static CopyrightInfo CreateCopyrightInfo()
    {
        return new CopyrightInfo
        {
            Isbn = "978-7-1234-5678-9",
            Edition = "第一版",
            Copyright = "© 2024 测试作者",
            Rights = "保留所有权利",
            PublishDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
        };
    }

    public static IReadOnlyList<ChapterInfo> CreateChapters(int count = 3)
    {
        var chapters = new List<ChapterInfo>();
        for (var i = 0; i < count; i++)
        {
            chapters.Add(new ChapterInfo
            {
                Index = i,
                Title = $"第{i + 1}章 测试章节",
                Content = $"这是第{i + 1}章的内容。\n\n这是第二段落。\n\n这是第三段落。",
                IsHtml = false,
            });
        }

        return chapters;
    }

    public static ChapterInfo CreateChapterWithImages()
    {
        return new ChapterInfo
        {
            Index = 0,
            Title = "带图片的章节",
            Content = "这是开头的文字。\n\n这是中间的文字。\n\n这是结尾的文字。",
            IsHtml = false,
            Images =
            [
                ChapterImageInfo.FromBytes("img001", 10, CreateMinimalPng(), "image/png", "测试图片1"),
                ChapterImageInfo.FromBytes("img002", 30, CreateMinimalPng(), "image/png", "测试图片2"),
            ],
        };
    }

    public static ChapterInfo CreateHtmlChapter()
    {
        return new ChapterInfo
        {
            Index = 0,
            Title = "HTML章节",
            Content = "<p>这是一个<strong>HTML</strong>段落。</p>\n<p>这是第二个段落。</p>",
            IsHtml = true,
        };
    }

    public static EpubOptions CreateDefaultOptions()
    {
        return new EpubOptions
        {
            Version = EpubVersion.Epub2,
            Direction = WritingDirection.Ltr,
            PageProgression = PageProgression.Ltr,
            IncludeTocPage = true,
            IncludeCopyrightPage = false,
        };
    }

    public static EpubOptions CreateEpub3Options()
    {
        return new EpubOptions
        {
            Version = EpubVersion.Epub3,
            Direction = WritingDirection.Ltr,
            PageProgression = PageProgression.Ltr,
            IncludeTocPage = true,
            IncludeCopyrightPage = true,
        };
    }

    private static byte[] CreateMinimalPng()
    {
        // 最小的有效 1x1 透明 PNG
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
}

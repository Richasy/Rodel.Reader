// Copyright (c) Reader Copilot. All rights reserved.

namespace EpubGenerator.Test.Validation;

[TestClass]
public sealed class EpubValidatorTests
{
    private EpubValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new EpubValidator();
    }

    #region ValidateInput Tests

    [TestMethod]
    public void ValidateInput_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author",
            Language = "zh",
            Identifier = "test-id-123"
        };

        var chapters = new List<ChapterInfo>
        {
            new() { Title = "Chapter 1", Content = "Content 1", Index = 0 }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.Errors);
        Assert.IsNull(result.Warnings);
    }

    [TestMethod]
    public void ValidateInput_WithMissingTitle_ShouldReturnError()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "",
            Author = "Test Author"
        };

        var chapters = new List<ChapterInfo>
        {
            new() { Title = "Chapter 1", Content = "Content 1", Index = 0 }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E001" && e.Message.Contains("标题", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void ValidateInput_WithMissingAuthor_ShouldReturnWarning()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = null,
            Identifier = "test-id"
        };

        var chapters = new List<ChapterInfo>
        {
            new() { Title = "Chapter 1", Content = "Content 1", Index = 0 }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsNotNull(result.Warnings);
        Assert.IsTrue(result.Warnings.Any(w => w.Code == "W001"));
    }

    [TestMethod]
    public void ValidateInput_WithEmptyChapters_ShouldReturnError()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author"
        };

        var chapters = new List<ChapterInfo>();

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E002"));
    }

    [TestMethod]
    public void ValidateInput_WithChapterMissingTitle_ShouldReturnError()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author"
        };

        var chapters = new List<ChapterInfo>
        {
            new() { Title = "", Content = "Content", Index = 0 }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E003" && e.Message.Contains("标题", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void ValidateInput_WithEmptyChapterContent_ShouldReturnWarning()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author",
            Identifier = "test-id"
        };

        var chapters = new List<ChapterInfo>
        {
            new() { Title = "Chapter 1", Content = "", Index = 0 }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsNotNull(result.Warnings);
        Assert.IsTrue(result.Warnings.Any(w => w.Code == "W004"));
    }

    [TestMethod]
    public void ValidateInput_WithNullMetadata_ShouldThrowArgumentNullException()
    {
        // Arrange
        var chapters = new List<ChapterInfo>
        {
            new() { Title = "Chapter 1", Content = "Content", Index = 0 }
        };

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => _validator.ValidateInput(null!, chapters));
    }

    [TestMethod]
    public void ValidateInput_WithNullChapters_ShouldThrowArgumentNullException()
    {
        // Arrange
        var metadata = new EpubMetadata { Title = "Test Book" };

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => _validator.ValidateInput(metadata, null!));
    }

    #endregion

    #region Anchor Validation Tests

    [TestMethod]
    public void ValidateInput_WithValidAnchors_ShouldReturnSuccess()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author",
            Identifier = "test-id"
        };

        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "Content",
                Index = 0,
                Anchors = new List<AnchorInfo>
                {
                    new() { Id = "section1", Title = "Section 1" },
                    new() { Id = "section2", Title = "Section 2" }
                }
            }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateInput_WithAnchorMissingId_ShouldReturnError()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author"
        };

        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "Content",
                Index = 0,
                Anchors = new List<AnchorInfo>
                {
                    new() { Id = "", Title = "Section 1" }
                }
            }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E006" && e.Message.Contains("缺少 ID", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void ValidateInput_WithAnchorMissingTitle_ShouldReturnError()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author"
        };

        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "Content",
                Index = 0,
                Anchors = new List<AnchorInfo>
                {
                    new() { Id = "section1", Title = "" }
                }
            }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E006" && e.Message.Contains("缺少标题", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void ValidateInput_WithDuplicateAnchorIds_ShouldReturnError()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author"
        };

        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "Content",
                Index = 0,
                Anchors = new List<AnchorInfo>
                {
                    new() { Id = "section1", Title = "Section 1" },
                    new() { Id = "section1", Title = "Section 1 Duplicate" }
                }
            }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E006" && e.Message.Contains("重复", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void ValidateInput_WithInvalidAnchorIdFormat_ShouldReturnError()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author"
        };

        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "Content",
                Index = 0,
                Anchors = new List<AnchorInfo>
                {
                    new() { Id = "123invalid", Title = "Invalid ID" } // ID 不能以数字开头
                }
            }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E006" && e.Message.Contains("格式", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void ValidateInput_WithValidXmlIdFormats_ShouldReturnSuccess()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author",
            Identifier = "test-id"
        };

        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "Content",
                Index = 0,
                Anchors = new List<AnchorInfo>
                {
                    new() { Id = "_underscore", Title = "Underscore start" },
                    new() { Id = "letter123", Title = "With numbers" },
                    new() { Id = "with-dash", Title = "With dash" },
                    new() { Id = "with.dot", Title = "With dot" },
                    new() { Id = "CamelCase", Title = "Camel case" }
                }
            }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    #endregion

    #region Cover Validation Tests

    [TestMethod]
    public void ValidateInput_WithValidCover_ShouldReturnSuccess()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author",
            Identifier = "test-id",
            Cover = CoverInfo.FromBytes(new byte[] { 1, 2, 3 }, "image/jpeg")
        };

        var chapters = new List<ChapterInfo>
        {
            new() { Title = "Chapter 1", Content = "Content", Index = 0 }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateInput_WithEmptyCoverData_ShouldReturnError()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author",
            Cover = CoverInfo.FromBytes(Array.Empty<byte>(), "image/jpeg")
        };

        var chapters = new List<ChapterInfo>
        {
            new() { Title = "Chapter 1", Content = "Content", Index = 0 }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E004" && e.Message.Contains("数据为空", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void ValidateInput_WithCoverMissingMediaType_ShouldReturnError()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author",
            Cover = new CoverInfo { ImageData = new byte[] { 1, 2, 3 }, MediaType = "" }
        };

        var chapters = new List<ChapterInfo>
        {
            new() { Title = "Chapter 1", Content = "Content", Index = 0 }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E004" && e.Message.Contains("媒体类型", StringComparison.Ordinal)));
    }

    #endregion

    #region Chapter Image Validation Tests

    [TestMethod]
    public void ValidateInput_WithValidChapterImages_ShouldReturnSuccess()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author",
            Identifier = "test-id"
        };

        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "Content with image",
                Index = 0,
                Images = new List<ChapterImageInfo>
                {
                    ChapterImageInfo.FromBytes("img1", 0, new byte[] { 1, 2, 3 }, "image/png")
                }
            }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateInput_WithChapterImageMissingId_ShouldReturnError()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author"
        };

        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "Content",
                Index = 0,
                Images = new List<ChapterImageInfo>
                {
                    new() { Id = "", Offset = 0, ImageData = new byte[] { 1, 2, 3 }, MediaType = "image/png" }
                }
            }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E007" && e.Message.Contains("缺少 ID", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void ValidateInput_WithChapterImageEmptyData_ShouldReturnError()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author"
        };

        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "Content",
                Index = 0,
                Images = new List<ChapterImageInfo>
                {
                    new() { Id = "img1", Offset = 0, ImageData = Array.Empty<byte>(), MediaType = "image/png" }
                }
            }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E007" && e.Message.Contains("数据为空", StringComparison.Ordinal)));
    }

    #endregion

    #region Resource Validation Tests

    [TestMethod]
    public void ValidateInput_WithValidResources_ShouldReturnSuccess()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author",
            Identifier = "test-id"
        };

        var chapters = new List<ChapterInfo>
        {
            new() { Title = "Chapter 1", Content = "Content", Index = 0 }
        };

        var options = new EpubOptions
        {
            Resources = new List<ResourceInfo>
            {
                new() { Id = "font1", FileName = "font.ttf", Data = new byte[] { 1, 2, 3 }, MediaType = "font/ttf", Type = ResourceType.Font }
            }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters, options);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateInput_WithDuplicateResourceIds_ShouldReturnError()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "Test Book",
            Author = "Test Author"
        };

        var chapters = new List<ChapterInfo>
        {
            new() { Title = "Chapter 1", Content = "Content", Index = 0 }
        };

        var options = new EpubOptions
        {
            Resources = new List<ResourceInfo>
            {
                new() { Id = "res1", FileName = "file1.png", Data = new byte[] { 1 }, MediaType = "image/png", Type = ResourceType.Image },
                new() { Id = "res1", FileName = "file2.png", Data = new byte[] { 2 }, MediaType = "image/png", Type = ResourceType.Image }
            }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters, options);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E005" && e.Message.Contains("重复", StringComparison.Ordinal)));
    }

    #endregion

    #region ValidateContent Tests

    [TestMethod]
    public void ValidateContent_WithValidContent_ShouldReturnSuccess()
    {
        // Arrange
        var content = new EpubContent
        {
            Mimetype = "application/epub+zip",
            ContainerXml = "<container>...</container>",
            ContentOpf = "<package>...</package>",
            TocNcx = "<ncx>...</ncx>",
            TitlePage = "<html>...</html>",
            StyleSheet = "body { }",
            Chapters = new Dictionary<string, string>
            {
                ["chapter_0.xhtml"] = "<html>...</html>"
            }
        };

        // Act
        var result = _validator.ValidateContent(content);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateContent_WithEmptyMimetype_ShouldReturnError()
    {
        // Arrange
        var content = new EpubContent
        {
            Mimetype = "",
            ContainerXml = "<container>...</container>",
            ContentOpf = "<package>...</package>",
            TocNcx = "<ncx>...</ncx>",
            TitlePage = "<html>...</html>",
            StyleSheet = "body { }",
            Chapters = new Dictionary<string, string>
            {
                ["chapter_0.xhtml"] = "<html>...</html>"
            }
        };

        // Act
        var result = _validator.ValidateContent(content);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E012" && e.Message.Contains("Mimetype", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void ValidateContent_WithNullContent_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => _validator.ValidateContent(null!));
    }

    [TestMethod]
    public void ValidateContent_WithEmptyChapters_ShouldReturnError()
    {
        // Arrange
        var content = new EpubContent
        {
            Mimetype = "application/epub+zip",
            ContainerXml = "<container>...</container>",
            ContentOpf = "<package>...</package>",
            TocNcx = "<ncx>...</ncx>",
            TitlePage = "<html>...</html>",
            StyleSheet = "body { }",
            Chapters = new Dictionary<string, string>()
        };

        // Act
        var result = _validator.ValidateContent(content);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E002"));
    }

    #endregion

    #region ValidateFileAsync Tests

    [TestMethod]
    public async Task ValidateFileAsync_WithNonExistentFile_ShouldReturnError()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".epub");

        // Act
        var result = await _validator.ValidateFileAsync(filePath);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "E008" && e.Message.Contains("不存在", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task ValidateFileAsync_WithInvalidZipFile_ShouldReturnError()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".epub");
        await File.WriteAllTextAsync(filePath, "This is not a valid ZIP file");

        try
        {
            // Act
            var result = await _validator.ValidateFileAsync(filePath);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.Errors);
            Assert.IsTrue(result.Errors.Any(e => e.Code == "E008"));
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    public async Task ValidateFileAsync_WithEmptyFilePath_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => _validator.ValidateFileAsync(""));
    }

    [TestMethod]
    public async Task ValidateFileAsync_WithNullFilePath_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _validator.ValidateFileAsync(null!));
    }

    #endregion

    #region Multiple Errors and Warnings Tests

    [TestMethod]
    public void ValidateInput_WithMultipleIssues_ShouldReturnAllErrorsAndWarnings()
    {
        // Arrange
        var metadata = new EpubMetadata
        {
            Title = "", // 错误：缺少标题
            Author = null, // 警告：缺少作者
            Language = "", // 警告：缺少语言
            Identifier = null // 警告：缺少标识符
        };

        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "", // 警告：内容为空
                Index = 0,
                Anchors = new List<AnchorInfo>
                {
                    new() { Id = "", Title = "Bad anchor" } // 错误：锚点缺少 ID
                }
            }
        };

        // Act
        var result = _validator.ValidateInput(metadata, chapters);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.Errors);
        Assert.IsTrue(result.Errors.Count >= 2); // 至少有标题和锚点错误
        Assert.IsNotNull(result.Warnings);
        Assert.IsTrue(result.Warnings.Count >= 2); // 至少有作者和内容警告
    }

    #endregion
}

// Copyright (c) Richasy. All rights reserved.

namespace Fb2Parser.Test.Reader;

/// <summary>
/// Fb2Reader 单元测试。
/// </summary>
[TestClass]
public sealed class Fb2ReaderTests
{
    [TestMethod]
    public async Task ReadFromStringAsync_MinimalFb2_ParsesSuccessfully()
    {
        // Arrange
        var content = TestDataFactory.CreateMinimalFb2();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.IsNotNull(book);
        Assert.AreEqual("Test Book", book.Metadata.Title);
        Assert.AreEqual(1, book.Metadata.Authors.Count);
        Assert.AreEqual("John Doe", book.Metadata.Authors[0].GetDisplayName());
        Assert.AreEqual("en", book.Metadata.Language);
    }

    [TestMethod]
    public async Task ReadFromStringAsync_WithFullMetadata_ParsesAllFields()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithFullMetadata();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.IsNotNull(book);
        Assert.AreEqual("The Great Adventure", book.Metadata.Title);
        Assert.AreEqual(2, book.Metadata.Authors.Count);
        // First author has nickname, so GetDisplayName() returns nickname
        Assert.AreEqual("JohnnyD", book.Metadata.Authors[0].GetDisplayName());
        Assert.AreEqual("Jane Smith", book.Metadata.Authors[1].GetDisplayName());

        Assert.AreEqual(2, book.Metadata.Genres.Count);
        Assert.IsTrue(book.Metadata.Genres.Contains("science_fiction"));
        Assert.IsTrue(book.Metadata.Genres.Contains("adventure"));

        Assert.IsNotNull(book.Metadata.Description);
        Assert.IsTrue(book.Metadata.Description.Contains("great book"));

        Assert.IsNotNull(book.Metadata.Sequence);
        Assert.AreEqual("Space Saga", book.Metadata.Sequence.Name);
        Assert.AreEqual(1, book.Metadata.Sequence.Number);

        Assert.AreEqual(1, book.Metadata.Translators.Count);
        Assert.AreEqual("Alex Translator", book.Metadata.Translators[0].GetDisplayName());

        Assert.IsNotNull(book.Metadata.DocumentInfo);
        Assert.AreEqual("FB2 Creator", book.Metadata.DocumentInfo.ProgramUsed);
        Assert.AreEqual("unique-doc-id-12345", book.Metadata.DocumentInfo.Id);

        Assert.IsNotNull(book.Metadata.PublishInfo);
        Assert.AreEqual("Amazing Books Publishing", book.Metadata.PublishInfo.Publisher);
        Assert.AreEqual("978-1234567890", book.Metadata.PublishInfo.Isbn);
    }

    [TestMethod]
    public async Task ReadFromStringAsync_WithNestedSections_ParsesHierarchy()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithNestedSections();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.AreEqual(2, book.Sections.Count);

        var part1 = book.Sections[0];
        Assert.AreEqual("Part 1", part1.Title);
        Assert.AreEqual("part1", part1.Id);
        Assert.AreEqual(2, part1.Children.Count);

        var chapter1 = part1.Children[0];
        Assert.AreEqual("Chapter 1", chapter1.Title);
        Assert.AreEqual(2, chapter1.Children.Count);

        var section11 = chapter1.Children[0];
        Assert.AreEqual("Section 1.1", section11.Title);
        Assert.AreEqual(2, section11.Level);
    }

    [TestMethod]
    public async Task ReadFromStringAsync_WithCover_ParsesCover()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithCoverAndImages();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.IsNotNull(book.Cover);
        Assert.AreEqual("cover.jpg", book.Cover.ImageId);
        Assert.AreEqual("image/jpeg", book.Cover.MediaType);

        var coverData = await book.Cover.ReadContentAsync();
        Assert.IsNotNull(coverData);
        Assert.IsTrue(coverData.Length > 0);
    }

    [TestMethod]
    public async Task ReadFromStringAsync_WithImages_ParsesAllImages()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithCoverAndImages();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.AreEqual(2, book.Binaries.Count);
        Assert.AreEqual(2, book.Images.Count);
    }

    [TestMethod]
    public async Task ReadFromStringAsync_WithoutNamespace_ParsesSuccessfully()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithoutNamespace();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.IsNotNull(book);
        Assert.AreEqual("Book Without Namespace", book.Metadata.Title);
    }

    [TestMethod]
    public async Task ReadFromStringAsync_WithSpecialCharacters_ParsesCorrectly()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithSpecialCharacters();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.AreEqual("Книга с кириллицей", book.Metadata.Title);
        Assert.AreEqual("Иван", book.Metadata.Authors[0].FirstName);
        Assert.AreEqual("Петров", book.Metadata.Authors[0].LastName);
        Assert.AreEqual("ru", book.Metadata.Language);
    }

    [TestMethod]
    public async Task ReadFromStringAsync_WithNicknameAuthor_UsesNickname()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithNicknameAuthor();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.AreEqual(1, book.Metadata.Authors.Count);
        Assert.AreEqual("PenName42", book.Metadata.Authors[0].GetDisplayName());
    }

    [TestMethod]
    public async Task ReadFromStringAsync_WithMultipleBodies_ParsesAll()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithMultipleBodies();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.IsTrue(book.Sections.Count >= 2);
    }

    [TestMethod]
    public async Task ReadFromStringAsync_InvalidXml_ThrowsFb2ParseException()
    {
        // Arrange
        var content = "not valid xml";

        // Act & Assert
        _ = await Assert.ThrowsExactlyAsync<Fb2ParseException>(async () =>
        {
            await Fb2Reader.ReadFromStringAsync(content);
        });
    }

    [TestMethod]
    public async Task ReadFromStringAsync_NotFictionBook_ThrowsFb2ParseException()
    {
        // Arrange
        var content = "<?xml version=\"1.0\"?><html><body>Not a FB2</body></html>";

        // Act & Assert
        var ex = await Assert.ThrowsExactlyAsync<Fb2ParseException>(async () =>
        {
            await Fb2Reader.ReadFromStringAsync(content);
        });

        Assert.IsTrue(ex.Message.Contains("FictionBook"));
    }

    [TestMethod]
    public async Task ReadAsync_FromStream_ParsesSuccessfully()
    {
        // Arrange
        var content = TestDataFactory.CreateMinimalFb2();
        using var stream = TestDataFactory.CreateStreamFromContent(content);

        // Act
        using var book = await Fb2Reader.ReadAsync(stream);

        // Assert
        Assert.IsNotNull(book);
        Assert.AreEqual("Test Book", book.Metadata.Title);
        Assert.IsNull(book.FilePath);
    }

    [TestMethod]
    public async Task ReadAsync_FromFile_ParsesSuccessfully()
    {
        // Arrange
        var content = TestDataFactory.CreateMinimalFb2();
        var tempFile = TestDataFactory.SaveToTempFile(content);

        try
        {
            // Act
            using var book = await Fb2Reader.ReadAsync(tempFile);

            // Assert
            Assert.IsNotNull(book);
            Assert.AreEqual("Test Book", book.Metadata.Title);
            Assert.AreEqual(tempFile, book.FilePath);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [TestMethod]
    public async Task ReadAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.fb2");

        // Act & Assert
        _ = await Assert.ThrowsExactlyAsync<FileNotFoundException>(async () =>
        {
            await Fb2Reader.ReadAsync(nonExistentPath);
        });
    }

    [TestMethod]
    public void Read_Synchronous_ParsesSuccessfully()
    {
        // Arrange
        var content = TestDataFactory.CreateMinimalFb2();
        var tempFile = TestDataFactory.SaveToTempFile(content);

        try
        {
            // Act
            using var book = Fb2Reader.Read(tempFile);

            // Assert
            Assert.IsNotNull(book);
            Assert.AreEqual("Test Book", book.Metadata.Title);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [TestMethod]
    public void ReadFromString_Synchronous_ParsesSuccessfully()
    {
        // Arrange
        var content = TestDataFactory.CreateMinimalFb2();

        // Act
        using var book = Fb2Reader.ReadFromString(content);

        // Assert
        Assert.IsNotNull(book);
        Assert.AreEqual("Test Book", book.Metadata.Title);
    }

    [TestMethod]
    public async Task ReadFromStringAsync_NavigationExtracted_MatchesSections()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithNestedSections();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.AreEqual(book.Sections.Count, book.Navigation.Count);

        var firstNav = book.Navigation[0];
        var firstSection = book.Sections[0];
        Assert.AreEqual(firstSection.Title, firstNav.Title);
        Assert.AreEqual(firstSection.Id, firstNav.SectionId);
    }

    [TestMethod]
    public async Task ReadFromStringAsync_SectionPlainText_ExtractedCorrectly()
    {
        // Arrange
        var content = TestDataFactory.CreateMinimalFb2();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.IsTrue(book.Sections.Count > 0);
        var section = book.Sections[0];
        Assert.IsTrue(section.PlainText.Contains("first paragraph"));
    }

    [TestMethod]
    public async Task ReadFromStringAsync_SectionImageIds_Extracted()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithCoverAndImages();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        var sectionWithImage = book.Sections.FirstOrDefault(s => s.ImageIds.Count > 0);
        Assert.IsNotNull(sectionWithImage);
        Assert.IsTrue(sectionWithImage.ImageIds.Contains("image1.png"));
    }

    [TestMethod]
    public async Task ReadFromStringAsync_EmptyBook_ParsesWithoutError()
    {
        // Arrange
        var content = TestDataFactory.CreateEmptyFb2();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.IsNotNull(book);
        Assert.AreEqual("Empty Book", book.Metadata.Title);
        Assert.AreEqual(0, book.Sections.Count);
    }

    [TestMethod]
    public async Task ReadFromStringAsync_Keywords_ParsedCorrectly()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithFullMetadata();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.AreEqual(3, book.Metadata.Keywords.Count);
        Assert.IsTrue(book.Metadata.Keywords.Contains("adventure"));
        Assert.IsTrue(book.Metadata.Keywords.Contains("science fiction"));
        Assert.IsTrue(book.Metadata.Keywords.Contains("space"));
    }
}

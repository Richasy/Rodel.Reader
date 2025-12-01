// Copyright (c) Richasy. All rights reserved.

namespace Fb2Parser.Test.Models;

/// <summary>
/// Fb2Metadata 单元测试。
/// </summary>
[TestClass]
public sealed class Fb2MetadataTests
{
    [TestMethod]
    public void Fb2Metadata_DefaultValues_AreCorrect()
    {
        // Act
        var metadata = new Fb2Metadata();

        // Assert
        Assert.IsNull(metadata.Title);
        Assert.IsNotNull(metadata.Authors);
        Assert.AreEqual(0, metadata.Authors.Count);
        Assert.IsNull(metadata.Description);
        Assert.IsNull(metadata.Publisher);
        Assert.IsNull(metadata.Language);
        Assert.IsNotNull(metadata.Genres);
        Assert.AreEqual(0, metadata.Genres.Count);
        Assert.IsNotNull(metadata.Keywords);
        Assert.AreEqual(0, metadata.Keywords.Count);
        Assert.IsNull(metadata.Sequence);
        Assert.IsNotNull(metadata.Translators);
        Assert.AreEqual(0, metadata.Translators.Count);
    }

    [TestMethod]
    public void Fb2Author_GetDisplayName_WithFullName_ReturnsFullName()
    {
        // Arrange
        var author = new Fb2Author
        {
            FirstName = "John",
            MiddleName = "William",
            LastName = "Doe",
        };

        // Act
        var displayName = author.GetDisplayName();

        // Assert
        Assert.AreEqual("John William Doe", displayName);
    }

    [TestMethod]
    public void Fb2Author_GetDisplayName_WithNickname_ReturnsNickname()
    {
        // Arrange
        var author = new Fb2Author
        {
            FirstName = "John",
            LastName = "Doe",
            Nickname = "JD",
        };

        // Act
        var displayName = author.GetDisplayName();

        // Assert
        Assert.AreEqual("JD", displayName);
    }

    [TestMethod]
    public void Fb2Author_GetDisplayName_OnlyFirstName_ReturnsFirstName()
    {
        // Arrange
        var author = new Fb2Author
        {
            FirstName = "John",
        };

        // Act
        var displayName = author.GetDisplayName();

        // Assert
        Assert.AreEqual("John", displayName);
    }

    [TestMethod]
    public void Fb2Author_GetDisplayName_Empty_ReturnsEmptyString()
    {
        // Arrange
        var author = new Fb2Author();

        // Act
        var displayName = author.GetDisplayName();

        // Assert
        Assert.AreEqual(string.Empty, displayName);
    }

    [TestMethod]
    public void Fb2Author_ToString_ReturnsDisplayName()
    {
        // Arrange
        var author = new Fb2Author
        {
            FirstName = "John",
            LastName = "Doe",
        };

        // Act
        var result = author.ToString();

        // Assert
        Assert.AreEqual("John Doe", result);
    }

    [TestMethod]
    public void Fb2Sequence_ToString_WithNumber_ReturnsNameAndNumber()
    {
        // Arrange
        var sequence = new Fb2Sequence
        {
            Name = "Space Saga",
            Number = 3,
        };

        // Act
        var result = sequence.ToString();

        // Assert
        Assert.AreEqual("Space Saga #3", result);
    }

    [TestMethod]
    public void Fb2Sequence_ToString_WithoutNumber_ReturnsName()
    {
        // Arrange
        var sequence = new Fb2Sequence
        {
            Name = "Space Saga",
        };

        // Act
        var result = sequence.ToString();

        // Assert
        Assert.AreEqual("Space Saga", result);
    }

    [TestMethod]
    public void Fb2DocumentInfo_DefaultValues_AreCorrect()
    {
        // Act
        var info = new Fb2DocumentInfo();

        // Assert
        Assert.IsNotNull(info.Authors);
        Assert.AreEqual(0, info.Authors.Count);
        Assert.IsNull(info.ProgramUsed);
        Assert.IsNull(info.Date);
        Assert.IsNotNull(info.SourceUrls);
        Assert.AreEqual(0, info.SourceUrls.Count);
        Assert.IsNull(info.Id);
        Assert.IsNull(info.Version);
    }

    [TestMethod]
    public void Fb2PublishInfo_DefaultValues_AreCorrect()
    {
        // Act
        var info = new Fb2PublishInfo();

        // Assert
        Assert.IsNull(info.BookName);
        Assert.IsNull(info.Publisher);
        Assert.IsNull(info.City);
        Assert.IsNull(info.Year);
        Assert.IsNull(info.Isbn);
        Assert.IsNull(info.Sequence);
    }
}

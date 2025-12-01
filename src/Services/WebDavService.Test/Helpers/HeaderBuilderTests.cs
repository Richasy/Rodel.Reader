// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav.Test.Helpers;

[TestClass]
public class HeaderBuilderTests
{
    [TestMethod]
    public void Build_EmptyBuilder_ReturnsEmptyDictionary()
    {
        // Arrange
        var builder = new HeaderBuilder();

        // Act
        var result = builder.Build();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Add_SingleHeader_ReturnsCorrectDictionary()
    {
        // Arrange
        var builder = new HeaderBuilder();

        // Act
        var result = builder.Add("X-Custom", "value").Build();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("value", result["X-Custom"]);
    }

    [TestMethod]
    public void Add_MultipleHeaders_ReturnsAllHeaders()
    {
        // Arrange
        var builder = new HeaderBuilder();

        // Act
        var result = builder
            .Add("Header1", "Value1")
            .Add("Header2", "Value2")
            .Build();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Value1", result["Header1"]);
        Assert.AreEqual("Value2", result["Header2"]);
    }

    [TestMethod]
    public void AddWithOverwrite_OverwritesExistingHeaders()
    {
        // Arrange
        var builder = new HeaderBuilder();
        var overwrite = new Dictionary<string, string>
        {
            { "Header1", "NewValue" },
            { "Header3", "Value3" },
        };

        // Act
        var result = builder
            .Add("Header1", "OldValue")
            .Add("Header2", "Value2")
            .AddWithOverwrite(overwrite)
            .Build();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("NewValue", result["Header1"]);
        Assert.AreEqual("Value2", result["Header2"]);
        Assert.AreEqual("Value3", result["Header3"]);
    }

    [TestMethod]
    public void AddWithOverwrite_NullDictionary_NoChanges()
    {
        // Arrange
        var builder = new HeaderBuilder();

        // Act
        var result = builder
            .Add("Header1", "Value1")
            .AddWithOverwrite(null)
            .Build();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
    }
}

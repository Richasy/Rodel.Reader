// Copyright (c) Richasy. All rights reserved.

namespace Fb2Parser.Test.Internal;

/// <summary>
/// NavigationExtractor 单元测试。
/// </summary>
[TestClass]
public sealed class NavigationExtractorTests
{
    [TestMethod]
    public void Extract_EmptyList_ReturnsEmptyList()
    {
        // Arrange
        var sections = new List<Fb2Section>();

        // Act
        var result = NavigationExtractor.Extract(sections);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Extract_SingleSection_CreatesNavItem()
    {
        // Arrange
        var sections = new List<Fb2Section>
        {
            new Fb2Section
            {
                Id = "ch1",
                Title = "Chapter 1",
                Level = 0,
            },
        };

        // Act
        var result = NavigationExtractor.Extract(sections);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Chapter 1", result[0].Title);
        Assert.AreEqual("ch1", result[0].SectionId);
        Assert.AreEqual(0, result[0].Level);
    }

    [TestMethod]
    public void Extract_NestedSections_CreatesHierarchy()
    {
        // Arrange
        var sections = new List<Fb2Section>
        {
            new Fb2Section
            {
                Id = "part1",
                Title = "Part 1",
                Level = 0,
                Children =
                [
                    new Fb2Section
                    {
                        Id = "ch1",
                        Title = "Chapter 1",
                        Level = 1,
                    },
                    new Fb2Section
                    {
                        Id = "ch2",
                        Title = "Chapter 2",
                        Level = 1,
                    },
                ],
            },
        };

        // Act
        var result = NavigationExtractor.Extract(sections);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(2, result[0].Children.Count);
        Assert.AreEqual("Chapter 1", result[0].Children[0].Title);
        Assert.AreEqual("Chapter 2", result[0].Children[1].Title);
    }

    [TestMethod]
    public void Extract_SectionWithoutTitle_SkipsIfNoChildren()
    {
        // Arrange
        var sections = new List<Fb2Section>
        {
            new Fb2Section
            {
                Id = "unnamed",
                Title = null,
                Level = 0,
            },
        };

        // Act
        var result = NavigationExtractor.Extract(sections);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Extract_SectionWithoutTitleButWithChildren_Included()
    {
        // Arrange
        var sections = new List<Fb2Section>
        {
            new Fb2Section
            {
                Id = "unnamed",
                Title = null,
                Level = 0,
                Children =
                [
                    new Fb2Section
                    {
                        Id = "ch1",
                        Title = "Chapter 1",
                        Level = 1,
                    },
                ],
            },
        };

        // Act
        var result = NavigationExtractor.Extract(sections);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result[0].Title.Contains("unnamed")); // Falls back to Section {Id}
    }

    [TestMethod]
    public void Extract_MultipleSections_PreservesOrder()
    {
        // Arrange
        var sections = new List<Fb2Section>
        {
            new Fb2Section { Title = "First", Level = 0 },
            new Fb2Section { Title = "Second", Level = 0 },
            new Fb2Section { Title = "Third", Level = 0 },
        };

        // Act
        var result = NavigationExtractor.Extract(sections);

        // Assert
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("First", result[0].Title);
        Assert.AreEqual("Second", result[1].Title);
        Assert.AreEqual("Third", result[2].Title);
    }

    [TestMethod]
    public void Extract_DeeplyNested_PreservesAllLevels()
    {
        // Arrange
        var sections = new List<Fb2Section>
        {
            new Fb2Section
            {
                Title = "Level 0",
                Level = 0,
                Children =
                [
                    new Fb2Section
                    {
                        Title = "Level 1",
                        Level = 1,
                        Children =
                        [
                            new Fb2Section
                            {
                                Title = "Level 2",
                                Level = 2,
                            },
                        ],
                    },
                ],
            },
        };

        // Act
        var result = NavigationExtractor.Extract(sections);

        // Assert
        Assert.AreEqual("Level 0", result[0].Title);
        Assert.AreEqual("Level 1", result[0].Children[0].Title);
        Assert.AreEqual("Level 2", result[0].Children[0].Children[0].Title);
    }

    [TestMethod]
    public void Extract_HasChildren_SetCorrectly()
    {
        // Arrange
        var sections = new List<Fb2Section>
        {
            new Fb2Section
            {
                Title = "Parent",
                Level = 0,
                Children =
                [
                    new Fb2Section { Title = "Child", Level = 1 },
                ],
            },
            new Fb2Section
            {
                Title = "Standalone",
                Level = 0,
            },
        };

        // Act
        var result = NavigationExtractor.Extract(sections);

        // Assert
        Assert.IsTrue(result[0].HasChildren);
        Assert.IsFalse(result[1].HasChildren);
    }
}

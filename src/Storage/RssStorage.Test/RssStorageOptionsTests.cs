// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Test;

/// <summary>
/// RssStorageOptions 测试.
/// </summary>
[TestClass]
public class RssStorageOptionsTests
{
    [TestMethod]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var options = new RssStorageOptions();

        // Assert
        Assert.AreEqual(string.Empty, options.DatabasePath);
        Assert.IsTrue(options.CreateTablesOnInit);
        Assert.AreEqual(30, options.DefaultArticleRetentionDays);
    }

    [TestMethod]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new RssStorageOptions
        {
            DatabasePath = "test.db",
            CreateTablesOnInit = false,
            DefaultArticleRetentionDays = 60,
        };

        // Assert
        Assert.AreEqual("test.db", options.DatabasePath);
        Assert.IsFalse(options.CreateTablesOnInit);
        Assert.AreEqual(60, options.DefaultArticleRetentionDays);
    }
}

// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Test;

/// <summary>
/// PodcastStorageOptions 测试.
/// </summary>
[TestClass]
public class PodcastStorageOptionsTests
{
    [TestMethod]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new PodcastStorageOptions();

        // Assert
        Assert.AreEqual("podcast.db", options.DatabasePath);
        Assert.IsTrue(options.CreateTablesOnInit);
    }

    [TestMethod]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new PodcastStorageOptions
        {
            DatabasePath = "/custom/path/podcast.db",
            CreateTablesOnInit = false,
        };

        // Assert
        Assert.AreEqual("/custom/path/podcast.db", options.DatabasePath);
        Assert.IsFalse(options.CreateTablesOnInit);
    }
}

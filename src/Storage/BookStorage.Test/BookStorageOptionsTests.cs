// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Test;

/// <summary>
/// BookStorageOptions 测试.
/// </summary>
[TestClass]
public class BookStorageOptionsTests
{
    [TestMethod]
    public void SetProperties_ShouldWork()
    {
        // Arrange
        var options = new BookStorageOptions
        {
            DatabasePath = "/path/to/db.sqlite",
            CreateTablesOnInit = false,
        };

        // Assert
        Assert.AreEqual("/path/to/db.sqlite", options.DatabasePath);
        Assert.IsFalse(options.CreateTablesOnInit);
    }
}

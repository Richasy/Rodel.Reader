// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Test;

/// <summary>
/// 书籍分组测试.
/// </summary>
[TestClass]
public class GroupCrudTests
{
    private string _testDbPath = null!;
    private BookStorage _storage = null!;
    private Shelf _testShelf = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"book_test_{Guid.NewGuid()}.db");
        var options = new BookStorageOptions
        {
            DatabasePath = _testDbPath,
            CreateTablesOnInit = true,
        };
        _storage = new BookStorage(options);
        await _storage.InitializeAsync();

        // 创建测试书架
        _testShelf = new Shelf
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = "Test Shelf",
            SortIndex = 0,
        };
        await _storage.UpsertShelfAsync(_testShelf);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _storage.DisposeAsync();
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    [TestMethod]
    public async Task UpsertGroup_Insert_ShouldSucceed()
    {
        // Arrange
        var group = CreateTestGroup();

        // Act
        await _storage.UpsertGroupAsync(group);
        var retrieved = await _storage.GetGroupAsync(group.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(group.Id, retrieved.Id);
        Assert.AreEqual(group.Name, retrieved.Name);
        Assert.AreEqual(group.ShelfId, retrieved.ShelfId);
    }

    [TestMethod]
    public async Task UpsertGroup_Update_ShouldSucceed()
    {
        // Arrange
        var group = CreateTestGroup();
        await _storage.UpsertGroupAsync(group);

        // Act
        group.Name = "Updated Group Name";
        group.IsCollapsed = true;
        await _storage.UpsertGroupAsync(group);
        var retrieved = await _storage.GetGroupAsync(group.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Updated Group Name", retrieved.Name);
        Assert.IsTrue(retrieved.IsCollapsed);
    }

    [TestMethod]
    public async Task GetGroupsByShelf_ShouldReturnOnlyShelfGroups()
    {
        // Arrange
        var group1 = CreateTestGroup("group1");
        var group2 = CreateTestGroup("group2");

        // 创建另一个书架及其分组
        var otherShelf = new Shelf
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = "Other Shelf",
            SortIndex = 1,
        };
        await _storage.UpsertShelfAsync(otherShelf);
        var otherGroup = new BookGroup
        {
            Id = Guid.NewGuid().ToString("N"),
            ShelfId = otherShelf.Id,
            Name = "Other Group",
            SortIndex = 0,
        };

        await _storage.UpsertGroupAsync(group1);
        await _storage.UpsertGroupAsync(group2);
        await _storage.UpsertGroupAsync(otherGroup);

        // Act
        var groups = await _storage.GetGroupsByShelfAsync(_testShelf.Id);

        // Assert
        Assert.AreEqual(2, groups.Count);
        Assert.IsTrue(groups.All(g => g.ShelfId == _testShelf.Id));
    }

    [TestMethod]
    public async Task DeleteGroup_ShouldRemoveGroup()
    {
        // Arrange
        var group = CreateTestGroup();
        await _storage.UpsertGroupAsync(group);

        // Act
        var deleted = await _storage.DeleteGroupAsync(group.Id);
        var retrieved = await _storage.GetGroupAsync(group.Id);

        // Assert
        Assert.IsTrue(deleted);
        Assert.IsNull(retrieved);
    }

    [TestMethod]
    public async Task DeleteGroup_ShouldMoveLinksOutOfGroup()
    {
        // Arrange
        var group = CreateTestGroup();
        await _storage.UpsertGroupAsync(group);

        var book = new Book
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = "Test Book",
            Format = BookFormat.Epub,
            SourceType = BookSourceType.Local,
            AddedAt = DateTimeOffset.UtcNow,
        };
        await _storage.UpsertBookAsync(book);

        // 添加书籍到分组
        await _storage.AddBookToShelfAsync(book.Id, _testShelf.Id, group.Id);

        // Act - 删除分组
        await _storage.DeleteGroupAsync(group.Id);

        // Assert - 书籍关联应该还在书架上但不在分组中
        var shelfBooks = await _storage.GetBooksInShelfAsync(_testShelf.Id);
        Assert.AreEqual(1, shelfBooks.Count);
        Assert.IsNull(shelfBooks[0].Link.GroupId);
    }

    private BookGroup CreateTestGroup(string? id = null)
    {
        return new BookGroup
        {
            Id = id ?? Guid.NewGuid().ToString("N"),
            ShelfId = _testShelf.Id,
            Name = "Test Group",
            SortIndex = 0,
            IsCollapsed = false,
        };
    }
}

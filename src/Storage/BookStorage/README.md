# BookStorage

书籍存储模块，提供本地书籍管理的数据持久化能力。

## 功能特性

- **统一书籍管理**：支持 EPUB、PDF、Mobi、FB2、TXT、漫画压缩包、网文等格式
- **书架与分组**：支持多书架管理，书架内支持书籍分组
- **阅读进度**：记录阅读位置和进度
- **阅读时长追踪**：详细记录每次阅读的开始时间、结束时间、阅读时长
- **书签与批注**：支持书签和高亮/批注功能

## 数据模型

### 核心实体

- `Book` - 书籍元数据
- `Shelf` - 书架
- `BookGroup` - 书籍分组（书架内）
- `ShelfBookLink` - 书架-书籍关联
- `ReadProgress` - 阅读进度
- `ReadingSession` - 阅读时段记录
- `Bookmark` - 书签
- `Annotation` - 批注/高亮

## 使用示例

```csharp
var options = new BookStorageOptions
{
    DatabasePath = "path/to/books.db",
    CreateTablesOnInit = true,
};

await using var storage = new BookStorage(options);
await storage.InitializeAsync();

// 添加书籍
var book = new Book
{
    Id = Guid.NewGuid().ToString(),
    Title = "示例书籍",
    Format = BookFormat.Epub,
    LocalPath = "/path/to/book.epub",
};
await storage.UpsertBookAsync(book);

// 记录阅读时段
var session = new ReadingSession
{
    Id = Guid.NewGuid().ToString(),
    BookId = book.Id,
    StartedAt = DateTimeOffset.Now.AddMinutes(-30),
    EndedAt = DateTimeOffset.Now,
    DurationSeconds = 1800,
};
await storage.AddReadingSessionAsync(session);
```

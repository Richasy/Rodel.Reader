# SqliteGenerator

一个轻量级的 SQLite Repository 代码生成器，基于 C# Source Generator 技术。

## 概述

`SqliteGenerator` 是一个编译时代码生成器，能够根据实体类的特性标记自动生成：

- SQL 语句常量（SELECT、INSERT、UPDATE、DELETE）
- 实体映射方法（DataReader → Entity）
- 参数绑定方法
- 完整的 CRUD 操作方法

## 特性

- 🚀 **编译时生成** - 零运行时开销
- ✅ **AOT 兼容** - 无反射依赖
- 🔒 **类型安全** - 编译时检查
- 📝 **使用 GetOrdinal** - 不依赖字段顺序
- ⚡ **自动时间戳** - 支持自动设置缓存时间

## 使用方法

### 1. 添加项目引用

```xml
<ItemGroup>
  <ProjectReference Include="..\SqliteGenerator\SqliteGenerator.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### 2. 定义实体类

```csharp
using Richasy.SqliteGenerator;

[SqliteTable("Articles")]
internal sealed partial class ArticleEntity
{
    [SqliteColumn("Id", IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn("Title")]
    public string Title { get; set; } = string.Empty;

    [SqliteColumn("Content", ExcludeFromList = true)]  // 列表查询时排除
    public string? Content { get; set; }

    [SqliteColumn("CachedAt", IsAutoTimestamp = true)]  // 自动设置时间戳
    public string CachedAt { get; set; } = string.Empty;
}
```

### 3. 自动生成的代码

Generator 会自动生成 `ArticleEntityRepository` 类，包含：

```csharp
internal sealed partial class ArticleEntityRepository
{
    // 字段列表
    private const string AllFields = "Id, Title, Content, CachedAt";
    private const string ListFields = "Id, Title, CachedAt";  // 排除了 Content

    // SQL 语句
    private const string SelectAllSql = "SELECT {0} FROM Articles";
    private const string SelectByIdSql = "SELECT {0} FROM Articles WHERE Id = @id";
    private const string UpsertSql = "INSERT INTO Articles (...) VALUES (...) ON CONFLICT...";
    private const string DeleteSql = "DELETE FROM Articles WHERE Id = @id";

    // 映射方法
    private static ArticleEntity MapToEntity(SqliteDataReader reader);
    private static ArticleEntity MapToEntityList(SqliteDataReader reader);

    // 参数方法
    private static void AddParameters(SqliteCommand cmd, ArticleEntity entity);

    // CRUD 方法
    public async Task<IReadOnlyList<ArticleEntity>> GetAllAsync(RssDatabase database, CancellationToken ct);
    public async Task<ArticleEntity?> GetByIdAsync(RssDatabase database, string id, CancellationToken ct);
    public async Task UpsertAsync(RssDatabase database, ArticleEntity entity, CancellationToken ct);
    public async Task UpsertManyAsync(RssDatabase database, IEnumerable<ArticleEntity> entities, CancellationToken ct);
    public async Task<bool> DeleteAsync(RssDatabase database, string id, CancellationToken ct);
}
```

## 特性说明

### `[SqliteTable]`

标记类为数据库表实体。

| 参数 | 说明 |
|------|------|
| `tableName` | 数据库表名 |

### `[SqliteColumn]`

标记属性为数据库列。

| 属性 | 类型 | 说明 |
|------|------|------|
| `columnName` | string? | 列名（默认使用属性名） |
| `IsPrimaryKey` | bool | 是否为主键 |
| `ExcludeFromList` | bool | 列表查询时是否排除（用于大文本字段） |
| `IsAutoTimestamp` | bool | 是否自动设置 UTC 时间戳 |

### `[SqliteIgnore]`

标记属性不映射到数据库。

## 查看生成的代码

在项目文件中添加以下配置可将生成的代码输出到磁盘：

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>

<!-- 排除生成的文件，避免重复编译 -->
<ItemGroup>
  <Compile Remove="Generated/**/*.cs" />
</ItemGroup>
```

生成的文件将位于 `Generated/Richasy.SqliteGenerator/` 目录下。

## 依赖

- `Microsoft.CodeAnalysis.CSharp` - Roslyn 编译器 API

## 许可证

Copyright (c) Richasy. All rights reserved.

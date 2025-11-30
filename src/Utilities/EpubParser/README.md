# EpubParser

一个轻量级、容错的 EPUB 解析库，专注于提取书籍的核心信息。

## 特性

- **轻量级**: 只关注核心功能——元数据、封面、目录、阅读顺序
- **容错性强**: 对不规范的 EPUB 文件有更好的兼容性
- **漫画支持**: 支持按阅读顺序获取图片内容
- **异步支持**: 提供同步和异步 API

## 安装

```csharp
// 项目引用
<ProjectReference Include="path/to/EpubParser.csproj" />
```

## 使用示例

### 基本用法

```csharp
using Richasy.EpubParser;

// 从文件读取
using var book = await EpubReader.ReadAsync("path/to/book.epub");

// 或从流读取
using var stream = File.OpenRead("path/to/book.epub");
using var book = await EpubReader.ReadAsync(stream);
```

### 获取元数据

```csharp
var metadata = book.Metadata;

Console.WriteLine($"标题: {metadata.Title}");
Console.WriteLine($"作者: {string.Join(", ", metadata.Authors)}");
Console.WriteLine($"描述: {metadata.Description}");
Console.WriteLine($"出版商: {metadata.Publisher}");
Console.WriteLine($"语言: {metadata.Language}");
Console.WriteLine($"ISBN: {metadata.Identifier}");
```

### 获取封面

```csharp
if (book.Cover != null)
{
    var coverData = await book.Cover.ReadContentAsync();
    // 使用封面图片数据
    File.WriteAllBytes("cover.jpg", coverData);
}
```

### 遍历目录

```csharp
void PrintToc(IReadOnlyList<EpubNavItem> items, int level = 0)
{
    foreach (var item in items)
    {
        var indent = new string(' ', level * 2);
        Console.WriteLine($"{indent}- {item.Title}");
        
        if (item.HasChildren)
        {
            PrintToc(item.Children, level + 1);
        }
    }
}

PrintToc(book.Navigation);
```

### 获取阅读顺序（适用于漫画）

```csharp
// 按阅读顺序遍历所有内容
foreach (var resource in book.ReadingOrder)
{
    Console.WriteLine($"{resource.Id}: {resource.Href}");
}

// 获取所有图片
foreach (var image in book.Images)
{
    Console.WriteLine($"图片: {image.Href}");
}
```

### 读取资源内容

```csharp
// 按需读取资源
var resource = book.FindResourceByHref("Images/cover.jpg");
if (resource != null)
{
    var content = await book.ReadResourceContentAsync(resource);
    // 使用内容...
}

// 读取为字符串（适用于 HTML/CSS）
var htmlResource = book.FindResourceByHref("Text/chapter1.xhtml");
if (htmlResource != null)
{
    var html = await book.ReadResourceContentAsStringAsync(htmlResource);
}
```

## API 参考

### EpubReader

| 方法 | 说明 |
|------|------|
| `Read(string filePath)` | 同步读取 EPUB 文件 |
| `ReadAsync(string filePath)` | 异步读取 EPUB 文件 |
| `Read(Stream stream)` | 从流同步读取 |
| `ReadAsync(Stream stream)` | 从流异步读取 |

### EpubBook

| 属性 | 类型 | 说明 |
|------|------|------|
| `Metadata` | `EpubMetadata` | 书籍元数据 |
| `Cover` | `EpubCover?` | 封面图片 |
| `Navigation` | `IReadOnlyList<EpubNavItem>` | 目录结构 |
| `ReadingOrder` | `IReadOnlyList<EpubResource>` | 阅读顺序 |
| `Resources` | `IReadOnlyList<EpubResource>` | 所有资源 |
| `Images` | `IReadOnlyList<EpubResource>` | 所有图片 |

### EpubMetadata

| 属性 | 类型 | 说明 |
|------|------|------|
| `Title` | `string?` | 标题 |
| `Authors` | `List<string>` | 作者列表 |
| `Description` | `string?` | 描述 |
| `Publisher` | `string?` | 出版商 |
| `Language` | `string?` | 语言 |
| `PublishDate` | `string?` | 出版日期 |
| `Identifier` | `string?` | 标识符 (ISBN等) |
| `Subjects` | `List<string>` | 主题/分类 |

### EpubNavItem

| 属性 | 类型 | 说明 |
|------|------|------|
| `Title` | `string` | 导航项标题 |
| `Href` | `string?` | 相对路径 |
| `FullPath` | `string?` | 完整路径 |
| `Anchor` | `string?` | 锚点 |
| `Children` | `List<EpubNavItem>` | 子项 |

### EpubResource

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` | `string` | 资源 ID |
| `Href` | `string` | 相对路径 |
| `FullPath` | `string` | 完整路径 |
| `MediaType` | `string` | MIME 类型 |
| `IsImage` | `bool` | 是否为图片 |
| `IsHtml` | `bool` | 是否为 HTML |

## 与 VersOne.Epub 的区别

| 特性 | VersOne.Epub | EpubParser |
|------|--------------|------------|
| 完整性 | 完整解析所有 EPUB 元素 | 只解析核心信息 |
| 容错性 | 严格验证 | 宽松解析 |
| 异常 | 多种细分异常 | 统一的 `EpubParseException` |
| SMIL | 支持 | 不支持 |
| 依赖 | 较多 | 仅标准库 |

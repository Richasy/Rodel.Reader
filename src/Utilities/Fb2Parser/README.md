# Fb2Parser

一个轻量级、容错的 FB2 (FictionBook 2.0) 解析库，专注于提取书籍的核心信息。

## 特性

- **轻量级**: 只关注核心功能——元数据、封面、目录、章节、图片
- **容错性强**: 对不规范的 FB2 文件有更好的兼容性
- **编码支持**: 自动检测和处理多种文本编码
- **异步支持**: 提供同步和异步 API

## 安装

```csharp
// 项目引用
<ProjectReference Include="path/to/Fb2Parser.csproj" />
```

## 使用示例

### 基本用法

```csharp
using Richasy.RodelReader.Utilities.Fb2Parser;

// 从文件读取
using var book = await Fb2Reader.ReadAsync("path/to/book.fb2");

// 或从流读取
using var stream = File.OpenRead("path/to/book.fb2");
using var book = await Fb2Reader.ReadAsync(stream);

// 或从字符串读取
using var book = await Fb2Reader.ReadFromStringAsync(fb2Content);
```

### 获取元数据

```csharp
var metadata = book.Metadata;

Console.WriteLine($"标题: {metadata.Title}");
Console.WriteLine($"作者: {string.Join(", ", metadata.Authors.Select(a => a.GetDisplayName()))}");
Console.WriteLine($"描述: {metadata.Description}");
Console.WriteLine($"出版商: {metadata.Publisher}");
Console.WriteLine($"语言: {metadata.Language}");
Console.WriteLine($"类型: {string.Join(", ", metadata.Genres)}");

// 系列信息
if (metadata.Sequence != null)
{
    Console.WriteLine($"系列: {metadata.Sequence.Name} #{metadata.Sequence.Number}");
}
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
void PrintToc(IReadOnlyList<Fb2NavItem> items, int level = 0)
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

### 获取章节

```csharp
// 遍历顶级章节
foreach (var section in book.Sections)
{
    Console.WriteLine($"章节: {section.Title}");
    Console.WriteLine($"纯文本: {section.PlainText.Substring(0, Math.Min(100, section.PlainText.Length))}...");
    
    // 遍历子章节
    foreach (var child in section.Children)
    {
        Console.WriteLine($"  子章节: {child.Title}");
    }
}

// 获取所有章节的扁平列表
var allSections = book.GetAllSections();
Console.WriteLine($"总章节数: {allSections.Count}");
```

### 获取图片

```csharp
// 遍历所有图片
foreach (var image in book.Images)
{
    Console.WriteLine($"图片: {image.Id} ({image.MediaType}, ~{image.Size} bytes)");
}

// 读取图片内容
foreach (var image in book.Images)
{
    var data = await book.ReadBinaryContentAsync(image);
    File.WriteAllBytes($"{image.Id}.jpg", data);
}

// 通过 ID 读取图片
var coverData = await book.ReadBinaryContentAsync("cover.jpg");
```

## API 参考

### Fb2Reader

| 方法 | 描述 |
|------|------|
| `Read(string filePath)` | 同步从文件路径解析 FB2 文件 |
| `ReadAsync(string filePath)` | 异步从文件路径解析 FB2 文件 |
| `Read(Stream stream)` | 同步从流解析 FB2 文件 |
| `ReadAsync(Stream stream)` | 异步从流解析 FB2 文件 |
| `ReadFromString(string content)` | 同步从字符串解析 FB2 内容 |
| `ReadFromStringAsync(string content)` | 异步从字符串解析 FB2 内容 |

### Fb2Book

| 属性 | 类型 | 描述 |
|------|------|------|
| `FilePath` | `string?` | 文件路径（从流加载时为 null） |
| `Metadata` | `Fb2Metadata` | 书籍元数据 |
| `Cover` | `Fb2Cover?` | 封面（如果有） |
| `Navigation` | `IReadOnlyList<Fb2NavItem>` | 导航/目录 |
| `Sections` | `IReadOnlyList<Fb2Section>` | 章节列表 |
| `Binaries` | `IReadOnlyList<Fb2Binary>` | 所有二进制资源 |
| `Images` | `IReadOnlyList<Fb2Binary>` | 所有图片资源 |

| 方法 | 描述 |
|------|------|
| `FindBinaryById(string id)` | 根据 ID 查找二进制资源 |
| `ReadBinaryContent(Fb2Binary binary)` | 读取二进制资源内容 |
| `ReadBinaryContentAsync(Fb2Binary binary)` | 异步读取二进制资源内容 |
| `GetAllSections()` | 获取所有章节的扁平列表 |

### Fb2Metadata

| 属性 | 类型 | 描述 |
|------|------|------|
| `Title` | `string?` | 书籍标题 |
| `Authors` | `List<Fb2Author>` | 作者列表 |
| `Description` | `string?` | 书籍描述/注释 |
| `Publisher` | `string?` | 出版商 |
| `Language` | `string?` | 语言 |
| `PublishDate` | `string?` | 出版日期 |
| `Identifier` | `string?` | 唯一标识符（如 ISBN） |
| `Genres` | `List<string>` | 类型/分类列表 |
| `Keywords` | `List<string>` | 关键词列表 |
| `Sequence` | `Fb2Sequence?` | 系列信息 |
| `Translators` | `List<Fb2Author>` | 翻译者列表 |
| `DocumentInfo` | `Fb2DocumentInfo?` | 文档信息 |
| `PublishInfo` | `Fb2PublishInfo?` | 出版信息 |

### Fb2Author

| 属性 | 类型 | 描述 |
|------|------|------|
| `FirstName` | `string?` | 名 |
| `MiddleName` | `string?` | 中间名 |
| `LastName` | `string?` | 姓 |
| `Nickname` | `string?` | 昵称 |
| `HomePage` | `string?` | 主页 URL |
| `Email` | `string?` | 电子邮件 |

| 方法 | 描述 |
|------|------|
| `GetDisplayName()` | 获取显示名称 |

## 错误处理

```csharp
try
{
    using var book = await Fb2Reader.ReadAsync("path/to/book.fb2");
    // 使用书籍...
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"文件未找到: {ex.FileName}");
}
catch (Fb2ParseException ex)
{
    Console.WriteLine($"解析错误: {ex.Message}");
}
```

## FB2 格式说明

FB2 (FictionBook 2.0) 是一种基于 XML 的电子书格式，主要特点：

- **纯 XML 格式**: 无需解压，直接解析
- **结构化元数据**: 支持丰富的书籍信息
- **嵌入式资源**: 图片以 Base64 编码存储在文件中
- **多语言支持**: 支持多种字符编码

### 文件结构

```xml
<?xml version="1.0" encoding="utf-8"?>
<FictionBook xmlns="http://www.gribuser.ru/xml/fictionbook/2.0">
    <description>
        <title-info>
            <author>...</author>
            <book-title>...</book-title>
            <annotation>...</annotation>
            <coverpage><image l:href="#cover.jpg"/></coverpage>
        </title-info>
        <document-info>...</document-info>
        <publish-info>...</publish-info>
    </description>
    <body>
        <section>
            <title><p>Chapter 1</p></title>
            <p>Content...</p>
        </section>
    </body>
    <binary id="cover.jpg" content-type="image/jpeg">
        /9j/4AAQSkZJRg...
    </binary>
</FictionBook>
```

## 许可证

MIT License

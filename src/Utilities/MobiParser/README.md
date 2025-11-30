# MobiParser

一个轻量级、容错的 Mobi/AZW 解析库，专注于提取书籍的核心信息。

## 特性

- **轻量级**: 只关注核心功能——元数据、封面、目录、图片
- **容错性强**: 对不规范的 Mobi 文件有更好的兼容性
- **漫画支持**: 支持按顺序获取图片内容
- **异步支持**: 提供同步和异步 API
- **格式支持**: 支持 .mobi, .azw, .azw3 文件

## 安装

```csharp
// 项目引用
<ProjectReference Include="path/to/MobiParser.csproj" />
```

## 使用示例

### 基本用法

```csharp
using Richasy.RodelReader.Utilities.MobiParser;

// 从文件读取
using var book = await MobiReader.ReadAsync("path/to/book.mobi");

// 或从流读取
using var stream = File.OpenRead("path/to/book.azw3");
using var book = await MobiReader.ReadAsync(stream);
```

### 获取元数据

```csharp
var metadata = book.Metadata;

Console.WriteLine($"标题: {metadata.Title}");
Console.WriteLine($"作者: {string.Join(", ", metadata.Authors)}");
Console.WriteLine($"描述: {metadata.Description}");
Console.WriteLine($"出版商: {metadata.Publisher}");
Console.WriteLine($"语言: {metadata.Language}");
Console.WriteLine($"ASIN: {metadata.Asin}");
Console.WriteLine($"ISBN: {metadata.Isbn}");
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
void PrintToc(IReadOnlyList<MobiNavItem> items, int level = 0)
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

### 获取图片（适用于漫画）

```csharp
// 遍历所有图片
foreach (var image in book.Images)
{
    Console.WriteLine($"图片 {image.Index}: {image.MediaType} ({image.Size} bytes)");
}

// 读取图片内容
foreach (var image in book.Images)
{
    var data = await book.ReadImageContentAsync(image);
    File.WriteAllBytes($"image_{image.Index}.jpg", data);
}
```

## API 参考

### MobiReader

| 方法 | 说明 |
|------|------|
| `Read(string filePath)` | 同步读取 Mobi 文件 |
| `ReadAsync(string filePath)` | 异步读取 Mobi 文件 |
| `Read(Stream stream)` | 从流同步读取 |
| `ReadAsync(Stream stream)` | 从流异步读取 |

### MobiBook

| 属性 | 类型 | 说明 |
|------|------|------|
| `FilePath` | `string?` | 文件路径（从流加载时为 null）|
| `Metadata` | `MobiMetadata` | 书籍元数据 |
| `Cover` | `MobiCover?` | 封面图片 |
| `Navigation` | `IReadOnlyList<MobiNavItem>` | 目录结构 |
| `Images` | `IReadOnlyList<MobiImage>` | 所有图片 |

| 方法 | 说明 |
|------|------|
| `ReadImageContent(int index)` | 读取指定索引的图片 |
| `ReadImageContentAsync(int index)` | 异步读取指定索引的图片 |
| `ReadImageContent(MobiImage image)` | 读取指定图片 |
| `ReadImageContentAsync(MobiImage image)` | 异步读取指定图片 |
| `FindImageByIndex(int index)` | 通过索引查找图片 |

### MobiMetadata

| 属性 | 类型 | 说明 |
|------|------|------|
| `Title` | `string?` | 标题 |
| `Authors` | `List<string>` | 作者列表 |
| `Description` | `string?` | 描述 |
| `Publisher` | `string?` | 出版商 |
| `Language` | `string?` | 语言 |
| `PublishDate` | `string?` | 出版日期 |
| `Identifier` | `string?` | 标识符 (ISBN/ASIN) |
| `Asin` | `string?` | 亚马逊标识号 |
| `Isbn` | `string?` | ISBN |
| `Subjects` | `List<string>` | 主题/分类 |
| `MobiVersion` | `int` | Mobi 版本 |

### MobiNavItem

| 属性 | 类型 | 说明 |
|------|------|------|
| `Title` | `string` | 导航项标题 |
| `Position` | `long` | 内容位置偏移 |
| `Anchor` | `string?` | 锚点 |
| `Children` | `List<MobiNavItem>` | 子项 |

### MobiImage

| 属性 | 类型 | 说明 |
|------|------|------|
| `Index` | `int` | 图片记录索引 |
| `MediaType` | `string` | MIME 类型 |
| `Size` | `int` | 图片大小（字节）|
| `IsValid` | `bool` | 是否为有效图片 |

## 与 EpubParser 的对比

| 特性 | EpubParser | MobiParser |
|------|------------|------------|
| 文件格式 | EPUB (.epub) | Mobi (.mobi, .azw, .azw3) |
| 存储结构 | ZIP 压缩包 | PalmDB 二进制格式 |
| 阅读顺序 | 有 (Spine) | 通过图片索引 |
| 目录 | NCX/Nav | HTML 提取 |
| 封面 | cover-image 属性 | EXTH CoverOffset |

## 支持的格式

- `.mobi` - 标准 Mobi 格式
- `.azw` - Kindle 格式 (本质上是 Mobi)
- `.azw3` - Kindle Format 8 (KF8)
- `.prc` - PalmDoc 格式

## 限制

- 不支持 DRM 加密的文件
- 不支持 HUFF/CDIC 压缩（较少使用）
- 目录提取依赖于 HTML 内容分析，可能不够精确

# EpubGenerator

一个高性能的 .NET 9 EPUB 电子书生成库，支持 EPUB 2 和 EPUB 3 标准。

## 特性

- 🚀 **高性能** - 使用 `Span<T>`、`StringBuilderPool` 等 .NET 9 优化技术
- 📖 **双版本支持** - 同时支持 EPUB 2 和 EPUB 3 标准
- 🔤 **多编码支持** - 自动检测 UTF-8、UTF-16、GB2312/GBK 等编码
- 🎨 **阅读器友好** - 不预设字体/颜色，让阅读器自由覆写样式
- 🧩 **接口优先** - 完全依赖注入友好，便于单元测试和扩展
- ✅ **充分测试** - 188+ 单元测试，包含真实书籍集成测试

## 快速开始

### 基本用法

```csharp
using Richasy.RodelPlayer.Utilities.EpubGenerator;

// 1. 创建构建器和分割器
var builder = new EpubBuilder();
var splitter = new RegexTextSplitter();

// 2. 从 TXT 文件分割章节
var chapters = await splitter.SplitFromFileAsync("novel.txt");

// 3. 设置元数据
var metadata = new EpubMetadata
{
    Title = "小说标题",
    Author = "作者名",
    Language = "zh",
};

// 4. 生成 EPUB
await builder.BuildToFileAsync(metadata, chapters, "output.epub");
```

### 自定义章节正则

```csharp
var options = new SplitOptions
{
    // 自定义章节匹配模式
    ChapterPattern = @"^第(\d+)章\s+(.+)$",
    
    // 额外的章节关键词
    ExtraChapterKeywords = ["序章", "前言", "后记"],
    
    // 去除空行
    RemoveEmptyLines = true,
};

var chapters = await splitter.SplitFromFileAsync("novel.txt", options);
```

### EPUB 选项

```csharp
var epubOptions = new EpubOptions
{
    // EPUB 版本
    Version = EpubVersion.Epub3,
    
    // 包含目录页
    IncludeTocPage = true,
    
    // 包含版权页
    IncludeCopyrightPage = true,
    
    // 自定义 CSS（追加到默认样式后）
    CustomCss = @"
        body { font-size: 1.2em; }
        .chapter-title { color: darkblue; }
    ",
};

await builder.BuildToFileAsync(metadata, chapters, "output.epub", epubOptions);
```

### 添加封面

```csharp
var coverData = await File.ReadAllBytesAsync("cover.jpg");

var metadata = new EpubMetadata
{
    Title = "小说标题",
    Author = "作者名",
    Cover = new CoverInfo
    {
        Data = coverData,
        MediaType = "image/jpeg",
    },
};
```

### 章节内嵌图片

```csharp
var chapters = new List<ChapterInfo>
{
    new()
    {
        Index = 0,
        Title = "第一章",
        Content = "正文内容...\n[IMG:img001]\n更多内容...",
        Images =
        [
            new ChapterImageInfo
            {
                Id = "img001",
                Data = imageBytes,
                MediaType = "image/png",
            }
        ],
    }
};
```

## 架构

### 核心接口

| 接口 | 描述 |
|------|------|
| `IEpubBuilder` | EPUB 构建器主入口 |
| `ITextSplitter` | 文本分割器 |
| `IEpubPackager` | EPUB 打包器 |
| `IEpubValidator` | EPUB 验证器 |

### 生成器接口

| 接口 | 描述 |
|------|------|
| `IContainerGenerator` | container.xml 生成 |
| `IOpfGenerator` | content.opf 生成 |
| `INcxGenerator` | toc.ncx 生成 (EPUB 2) |
| `INavDocGenerator` | nav.xhtml 生成 (EPUB 3) |
| `IChapterGenerator` | 章节 XHTML 生成 |
| `IStyleSheetGenerator` | CSS 样式表生成 |
| `ICoverPageGenerator` | 封面页生成 |
| `ITitlePageGenerator` | 标题页生成 |
| `ITocPageGenerator` | 目录页生成 |
| `ICopyrightPageGenerator` | 版权页生成 |

### 依赖注入示例

```csharp
services.AddSingleton<IContainerGenerator, ContainerGenerator>();
services.AddSingleton<IOpfGenerator, OpfGenerator>();
services.AddSingleton<INcxGenerator, NcxGenerator>();
services.AddSingleton<INavDocGenerator, NavDocGenerator>();
services.AddSingleton<IStyleSheetGenerator, StyleSheetGenerator>();
services.AddSingleton<ICoverPageGenerator, CoverPageGenerator>();
services.AddSingleton<ITitlePageGenerator, TitlePageGenerator>();
services.AddSingleton<ITocPageGenerator, TocPageGenerator>();
services.AddSingleton<ICopyrightPageGenerator, CopyrightPageGenerator>();
services.AddSingleton<IChapterGenerator, ChapterGenerator>();
services.AddSingleton<IEpubPackager, ZipEpubPackager>();
services.AddSingleton<IEpubBuilder, EpubBuilder>();
services.AddSingleton<ITextSplitter, RegexTextSplitter>();
services.AddSingleton<IEpubValidator, EpubValidator>();
```

## 默认样式

默认 CSS 样式专为阅读器兼容性设计：

- ✅ 不设置 `font-family` - 让阅读器决定字体
- ✅ 不设置 `color` - 让阅读器决定文字颜色
- ✅ 不设置 `background-color` - 支持夜间模式
- ✅ 使用 `opacity` 代替硬编码颜色
- ✅ 使用 `currentColor` 自适应当前颜色

## 编码支持

自动检测以下编码：

- UTF-8 (带/不带 BOM)
- UTF-16 LE/BE
- UTF-32 LE/BE
- GB2312/GBK (中文)

## 测试

```bash
cd src/Utilities/EpubGenerator.Test
dotnet test
```

### 测试覆盖

- **单元测试**: 167+ 测试覆盖所有生成器和验证器
- **集成测试**: 使用真实中文小说验证完整流程
  - 三国演义 (119 章)
  - 遮天 (1822 章)
  - 青云台 (215 章)

## 许可证

MIT License

## 相关链接

- [EPUB 3 规范](https://www.w3.org/TR/epub-33/)
- [EPUB 2 规范](http://idpf.org/epub/201)

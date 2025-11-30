// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 表示 EPUB 书籍的元数据。
/// </summary>
public sealed class EpubMetadata
{
    /// <summary>
    /// 获取或设置书籍标题。
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 获取或设置作者列表。
    /// </summary>
    public List<string> Authors { get; set; } = [];

    /// <summary>
    /// 获取或设置书籍描述。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 获取或设置出版商。
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// 获取或设置语言。
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// 获取或设置出版日期。
    /// </summary>
    public string? PublishDate { get; set; }

    /// <summary>
    /// 获取或设置唯一标识符（如 ISBN）。
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    /// 获取或设置书籍主题/分类列表。
    /// </summary>
    public List<string> Subjects { get; set; } = [];

    /// <summary>
    /// 获取或设置版权信息。
    /// </summary>
    public string? Rights { get; set; }

    /// <summary>
    /// 获取或设置贡献者列表。
    /// </summary>
    public List<string> Contributors { get; set; } = [];

    /// <summary>
    /// 获取或设置自定义元数据。
    /// 键为属性名称，值为属性内容。
    /// </summary>
    public Dictionary<string, string> CustomMetadata { get; set; } = [];

    /// <summary>
    /// 获取或设置所有 meta 元素。
    /// 用于存储 EPUB 中的扩展元数据。
    /// </summary>
    public List<EpubMetaItem> MetaItems { get; set; } = [];
}

/// <summary>
/// 表示 EPUB 元数据中的 meta 元素。
/// </summary>
public sealed class EpubMetaItem
{
    /// <summary>
    /// 获取或设置 name 属性（EPUB 2）。
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 获取或设置 content 属性（EPUB 2）或元素内容（EPUB 3）。
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 获取或设置 property 属性（EPUB 3）。
    /// </summary>
    public string? Property { get; set; }

    /// <summary>
    /// 获取或设置 refines 属性（EPUB 3）。
    /// </summary>
    public string? Refines { get; set; }

    /// <summary>
    /// 获取或设置 scheme 属性。
    /// </summary>
    public string? Scheme { get; set; }

    /// <summary>
    /// 获取或设置 id 属性。
    /// </summary>
    public string? Id { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"{Name ?? Property}: {Content}";
}

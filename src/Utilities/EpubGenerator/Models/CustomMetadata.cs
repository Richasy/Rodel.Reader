// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 自定义元数据.
/// </summary>
/// <remarks>
/// 用于在 EPUB OPF 文件中添加自定义 &lt;meta&gt; 标签.
/// 支持 EPUB 2 和 EPUB 3 两种格式的元数据.
/// </remarks>
public sealed class CustomMetadata
{
    /// <summary>
    /// 元数据名称（EPUB 2 使用 name 属性，EPUB 3 使用 property 属性）.
    /// </summary>
    /// <remarks>
    /// 对于 EPUB 2，这将生成: &lt;meta name="Name" content="Value"/&gt;
    /// 对于 EPUB 3，这将生成: &lt;meta property="Name"&gt;Value&lt;/meta&gt;
    /// </remarks>
    public required string Name { get; init; }

    /// <summary>
    /// 元数据值.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// 元数据方案/类型（可选，仅 EPUB 2）.
    /// </summary>
    /// <remarks>
    /// 例如: "UUID", "ISBN", "DOI" 等.
    /// 生成: &lt;meta name="Name" content="Value" scheme="Scheme"/&gt;
    /// </remarks>
    public string? Scheme { get; init; }

    /// <summary>
    /// 引用的元素 ID（可选，仅 EPUB 3）.
    /// </summary>
    /// <remarks>
    /// 用于 refines 属性，关联到其他元数据元素.
    /// 生成: &lt;meta property="Name" refines="#RefinesId"&gt;Value&lt;/meta&gt;
    /// </remarks>
    public string? RefinesId { get; init; }

    /// <summary>
    /// 创建简单的自定义元数据.
    /// </summary>
    public static CustomMetadata Create(string name, string value)
        => new() { Name = name, Value = value };

    /// <summary>
    /// 创建带方案的自定义元数据（EPUB 2）.
    /// </summary>
    public static CustomMetadata CreateWithScheme(string name, string value, string scheme)
        => new() { Name = name, Value = value, Scheme = scheme };

    /// <summary>
    /// 创建带引用的自定义元数据（EPUB 3）.
    /// </summary>
    public static CustomMetadata CreateWithRefines(string name, string value, string refinesId)
        => new() { Name = name, Value = value, RefinesId = refinesId };
}

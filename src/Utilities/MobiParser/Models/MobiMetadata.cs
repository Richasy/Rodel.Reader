// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// 表示 Mobi 书籍的元数据。
/// </summary>
public sealed class MobiMetadata
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
    /// 获取或设置唯一标识符（如 ISBN 或 ASIN）。
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
    /// 获取或设置 ASIN（亚马逊标准识别号）。
    /// </summary>
    public string? Asin { get; set; }

    /// <summary>
    /// 获取或设置 ISBN。
    /// </summary>
    public string? Isbn { get; set; }

    /// <summary>
    /// 获取或设置 Mobi 版本。
    /// </summary>
    public int MobiVersion { get; set; }

    /// <summary>
    /// 获取或设置自定义元数据。
    /// 键为属性名称，值为属性内容。
    /// </summary>
    public Dictionary<string, string> CustomMetadata { get; set; } = [];
}

// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// EPUB 书籍元数据.
/// </summary>
public sealed class EpubMetadata
{
    /// <summary>
    /// 书籍标题（必填）.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 作者.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// 语言代码（如 zh, en, ja）.
    /// </summary>
    public string Language { get; init; } = "zh";

    /// <summary>
    /// 书籍描述/简介.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 出版商.
    /// </summary>
    public string? Publisher { get; init; }

    /// <summary>
    /// 出版日期.
    /// </summary>
    public DateTimeOffset? PublishDate { get; init; }

    /// <summary>
    /// 书籍唯一标识符（ISBN 或 UUID，不提供则自动生成 UUID）.
    /// </summary>
    public string? Identifier { get; init; }

    /// <summary>
    /// 封面信息（可选）.
    /// </summary>
    public CoverInfo? Cover { get; init; }

    /// <summary>
    /// 版权信息（可选）.
    /// </summary>
    public CopyrightInfo? Copyright { get; init; }

    /// <summary>
    /// 书籍主题/分类标签.
    /// </summary>
    public IReadOnlyList<string>? Subjects { get; init; }

    /// <summary>
    /// 贡献者列表（译者、编辑等）.
    /// </summary>
    public IReadOnlyList<string>? Contributors { get; init; }

    /// <summary>
    /// 自定义元数据列表（可选）.
    /// </summary>
    /// <remarks>
    /// 用于添加自定义标识符或其他元数据，如来源标识、生成工具信息等.
    /// 这些元数据将作为 &lt;meta&gt; 标签添加到 OPF 文件中.
    /// </remarks>
    public IReadOnlyList<CustomMetadata>? CustomMetadata { get; init; }
}

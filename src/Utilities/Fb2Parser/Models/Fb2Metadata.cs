// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.Fb2Parser;

/// <summary>
/// 表示 FB2 书籍的元数据。
/// </summary>
public sealed class Fb2Metadata
{
    /// <summary>
    /// 获取或设置书籍标题。
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 获取或设置作者列表。
    /// </summary>
    public List<Fb2Author> Authors { get; set; } = [];

    /// <summary>
    /// 获取或设置书籍描述/注释。
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
    /// 获取或设置唯一标识符。
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    /// 获取或设置书籍类型/分类列表。
    /// </summary>
    public List<string> Genres { get; set; } = [];

    /// <summary>
    /// 获取或设置关键词列表。
    /// </summary>
    public List<string> Keywords { get; set; } = [];

    /// <summary>
    /// 获取或设置系列信息。
    /// </summary>
    public Fb2Sequence? Sequence { get; set; }

    /// <summary>
    /// 获取或设置翻译者列表。
    /// </summary>
    public List<Fb2Author> Translators { get; set; } = [];

    /// <summary>
    /// 获取或设置封面图片的引用 ID。
    /// </summary>
    public string? CoverpageImageId { get; set; }

    /// <summary>
    /// 获取或设置文档信息。
    /// </summary>
    public Fb2DocumentInfo? DocumentInfo { get; set; }

    /// <summary>
    /// 获取或设置出版信息。
    /// </summary>
    public Fb2PublishInfo? PublishInfo { get; set; }

    /// <summary>
    /// 获取或设置自定义元数据。
    /// </summary>
    public Dictionary<string, string> CustomMetadata { get; set; } = [];
}

/// <summary>
/// 表示 FB2 作者信息。
/// </summary>
public sealed class Fb2Author
{
    /// <summary>
    /// 获取或设置名。
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// 获取或设置中间名。
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// 获取或设置姓。
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// 获取或设置昵称。
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// 获取或设置主页 URL。
    /// </summary>
    public string? HomePage { get; set; }

    /// <summary>
    /// 获取或设置电子邮件。
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 获取或设置 ID。
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 获取显示名称。
    /// </summary>
    /// <returns>作者的显示名称。</returns>
    public string GetDisplayName()
    {
        if (!string.IsNullOrEmpty(Nickname))
        {
            return Nickname;
        }

        var parts = new List<string>();
        if (!string.IsNullOrEmpty(FirstName))
        {
            parts.Add(FirstName);
        }

        if (!string.IsNullOrEmpty(MiddleName))
        {
            parts.Add(MiddleName);
        }

        if (!string.IsNullOrEmpty(LastName))
        {
            parts.Add(LastName);
        }

        return parts.Count > 0 ? string.Join(" ", parts) : string.Empty;
    }

    /// <inheritdoc/>
    public override string ToString() => GetDisplayName();
}

/// <summary>
/// 表示 FB2 系列信息。
/// </summary>
public sealed class Fb2Sequence
{
    /// <summary>
    /// 获取或设置系列名称。
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 获取或设置在系列中的编号。
    /// </summary>
    public int? Number { get; set; }

    /// <inheritdoc/>
    public override string ToString() => Number.HasValue ? $"{Name} #{Number}" : Name ?? string.Empty;
}

/// <summary>
/// 表示 FB2 文档信息。
/// </summary>
public sealed class Fb2DocumentInfo
{
    /// <summary>
    /// 获取或设置文档作者（制作者）列表。
    /// </summary>
    public List<Fb2Author> Authors { get; set; } = [];

    /// <summary>
    /// 获取或设置创建程序名称。
    /// </summary>
    public string? ProgramUsed { get; set; }

    /// <summary>
    /// 获取或设置创建日期。
    /// </summary>
    public string? Date { get; set; }

    /// <summary>
    /// 获取或设置源 URL 列表。
    /// </summary>
    public List<string> SourceUrls { get; set; } = [];

    /// <summary>
    /// 获取或设置文档 ID。
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 获取或设置版本号。
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 获取或设置修改历史。
    /// </summary>
    public string? History { get; set; }
}

/// <summary>
/// 表示 FB2 出版信息。
/// </summary>
public sealed class Fb2PublishInfo
{
    /// <summary>
    /// 获取或设置书名。
    /// </summary>
    public string? BookName { get; set; }

    /// <summary>
    /// 获取或设置出版商。
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// 获取或设置出版城市。
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// 获取或设置出版年份。
    /// </summary>
    public string? Year { get; set; }

    /// <summary>
    /// 获取或设置 ISBN。
    /// </summary>
    public string? Isbn { get; set; }

    /// <summary>
    /// 获取或设置系列信息。
    /// </summary>
    public Fb2Sequence? Sequence { get; set; }
}

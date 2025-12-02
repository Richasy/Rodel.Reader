// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// 全文搜索选项.
/// </summary>
public sealed class FullTextSearchOptions
{
    /// <summary>
    /// 获取或设置是否精确匹配.
    /// </summary>
    public bool Exact { get; set; }

    /// <summary>
    /// 获取或设置是否匹配短语（至少 2 个单词）.
    /// </summary>
    public bool MatchPhrase { get; set; }

    /// <summary>
    /// 获取或设置是否匹配单词.
    /// </summary>
    public bool MatchWords { get; set; }

    /// <summary>
    /// 获取或设置起始年份.
    /// </summary>
    public int? FromYear { get; set; }

    /// <summary>
    /// 获取或设置结束年份.
    /// </summary>
    public int? ToYear { get; set; }

    /// <summary>
    /// 获取或设置语言筛选列表.
    /// </summary>
    public IReadOnlyList<BookLanguage>? Languages { get; set; }

    /// <summary>
    /// 获取或设置文件格式筛选列表.
    /// </summary>
    public IReadOnlyList<BookExtension>? Extensions { get; set; }

    /// <summary>
    /// 获取或设置每页数量.
    /// </summary>
    public int PageSize { get; set; } = 10;
}

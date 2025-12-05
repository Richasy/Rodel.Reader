// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss;

/// <summary>
/// RSS 存储配置选项.
/// </summary>
public sealed class RssStorageOptions
{
    /// <summary>
    /// 数据库文件路径.
    /// </summary>
    public string DatabasePath { get; set; } = string.Empty;

    /// <summary>
    /// 是否在初始化时创建表.
    /// </summary>
    public bool CreateTablesOnInit { get; set; } = true;

    /// <summary>
    /// 默认的文章保留天数（0 表示不自动清理）.
    /// </summary>
    public int DefaultArticleRetentionDays { get; set; } = 30;
}

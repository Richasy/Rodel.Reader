// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast;

/// <summary>
/// 播客存储选项.
/// </summary>
public sealed class PodcastStorageOptions
{
    /// <summary>
    /// 数据库文件路径.
    /// </summary>
    public string DatabasePath { get; set; } = "podcast.db";

    /// <summary>
    /// 是否在初始化时创建表.
    /// </summary>
    public bool CreateTablesOnInit { get; set; } = true;
}

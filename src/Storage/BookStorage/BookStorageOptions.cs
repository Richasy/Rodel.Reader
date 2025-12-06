// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 书籍存储选项.
/// </summary>
public sealed class BookStorageOptions
{
    /// <summary>
    /// 数据库文件路径.
    /// </summary>
    public required string DatabasePath { get; set; }

    /// <summary>
    /// 初始化时是否创建表.
    /// </summary>
    public bool CreateTablesOnInit { get; set; } = true;
}

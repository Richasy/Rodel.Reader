// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.ServiceRegistry.Database;

/// <summary>
/// 数据库 Schema 定义.
/// </summary>
internal static class Schema
{
    /// <summary>
    /// 创建所有表的 SQL.
    /// </summary>
    public const string CreateTablesSql = """
        -- 服务实例表
        CREATE TABLE IF NOT EXISTS Services (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            Type INTEGER NOT NULL,
            Icon TEXT,
            Color TEXT,
            Description TEXT,
            CreatedAt INTEGER NOT NULL,
            LastAccessedAt INTEGER NOT NULL,
            SortOrder INTEGER NOT NULL DEFAULT 0,
            Settings TEXT,
            IsActive INTEGER NOT NULL DEFAULT 0
        );

        -- 创建索引
        CREATE INDEX IF NOT EXISTS IX_Services_Type ON Services (Type);
        CREATE INDEX IF NOT EXISTS IX_Services_IsActive ON Services (IsActive);
        CREATE INDEX IF NOT EXISTS IX_Services_SortOrder ON Services (SortOrder);
        CREATE UNIQUE INDEX IF NOT EXISTS IX_Services_Name ON Services (Name);
        """;
}

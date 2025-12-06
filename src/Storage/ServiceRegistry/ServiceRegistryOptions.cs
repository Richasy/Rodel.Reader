// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.ServiceRegistry;

/// <summary>
/// 服务注册表配置选项.
/// </summary>
public sealed class ServiceRegistryOptions
{
    /// <summary>
    /// 库根目录路径.
    /// </summary>
    public required string LibraryPath { get; set; }

    /// <summary>
    /// 注册表数据库文件名.
    /// </summary>
    public string RegistryFileName { get; set; } = "registry.db";

    /// <summary>
    /// 是否在初始化时创建表.
    /// </summary>
    public bool CreateTablesOnInit { get; set; } = true;

    /// <summary>
    /// 获取注册表数据库完整路径.
    /// </summary>
    public string GetDatabasePath() => Path.Combine(LibraryPath, RegistryFileName);
}

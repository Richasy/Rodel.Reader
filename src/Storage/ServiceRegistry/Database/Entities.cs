// Copyright (c) Richasy. All rights reserved.

using Richasy.SqliteGenerator;

namespace Richasy.RodelReader.Storage.ServiceRegistry.Database;

/// <summary>
/// 服务实例实体（数据库映射）.
/// </summary>
[SqliteTable("Services")]
internal sealed partial class ServiceEntity
{
    /// <summary>
    /// 服务唯一标识符.
    /// </summary>
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 服务名称.
    /// </summary>
    [SqliteColumn]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 服务类型.
    /// </summary>
    [SqliteColumn]
    public int Type { get; set; }

    /// <summary>
    /// 图标标识.
    /// </summary>
    [SqliteColumn]
    public string? Icon { get; set; }

    /// <summary>
    /// 主题色.
    /// </summary>
    [SqliteColumn]
    public string? Color { get; set; }

    /// <summary>
    /// 服务描述.
    /// </summary>
    [SqliteColumn]
    public string? Description { get; set; }

    /// <summary>
    /// 创建时间（Unix 时间戳，毫秒）.
    /// </summary>
    [SqliteColumn]
    public long CreatedAt { get; set; }

    /// <summary>
    /// 最后访问时间（Unix 时间戳，毫秒）.
    /// </summary>
    [SqliteColumn]
    public long LastAccessedAt { get; set; }

    /// <summary>
    /// 排序顺序.
    /// </summary>
    [SqliteColumn]
    public int SortOrder { get; set; }

    /// <summary>
    /// 扩展配置 JSON.
    /// </summary>
    [SqliteColumn]
    public string? Settings { get; set; }

    /// <summary>
    /// 是否为当前活动服务.
    /// </summary>
    [SqliteColumn]
    public int IsActive { get; set; }

    /// <summary>
    /// 从模型创建实体.
    /// </summary>
    public static ServiceEntity FromModel(ServiceInstance model)
    {
        return new ServiceEntity
        {
            Id = model.Id,
            Name = model.Name,
            Type = (int)model.Type,
            Icon = model.Icon,
            Color = model.Color,
            Description = model.Description,
            CreatedAt = model.CreatedAt.ToUnixTimeMilliseconds(),
            LastAccessedAt = model.LastAccessedAt.ToUnixTimeMilliseconds(),
            SortOrder = model.SortOrder,
            Settings = model.Settings,
            IsActive = model.IsActive ? 1 : 0,
        };
    }

    /// <summary>
    /// 转换为模型.
    /// </summary>
    public ServiceInstance ToModel()
    {
        return new ServiceInstance
        {
            Id = Id,
            Name = Name,
            Type = (ServiceType)Type,
            Icon = Icon,
            Color = Color,
            Description = Description,
            CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(CreatedAt),
            LastAccessedAt = DateTimeOffset.FromUnixTimeMilliseconds(LastAccessedAt),
            SortOrder = SortOrder,
            Settings = Settings,
            IsActive = IsActive != 0,
        };
    }
}

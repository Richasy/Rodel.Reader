// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.ServiceRegistry;

/// <summary>
/// 服务实例.
/// </summary>
public sealed class ServiceInstance
{
    /// <summary>
    /// 服务唯一标识符.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 服务名称（用户自定义）.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 服务类型.
    /// </summary>
    public ServiceType Type { get; set; }

    /// <summary>
    /// 图标标识（可选，用于 UI 展示）.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 主题色（可选，十六进制颜色值）.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// 服务描述（可选）.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 最后访问时间.
    /// </summary>
    public DateTimeOffset LastAccessedAt { get; set; }

    /// <summary>
    /// 排序顺序（数字越小越靠前）.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 扩展配置（JSON 格式，用于存储特定服务的额外配置）.
    /// </summary>
    public string? Settings { get; set; }

    /// <summary>
    /// 是否为当前活动服务.
    /// </summary>
    public bool IsActive { get; set; }
}

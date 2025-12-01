// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 资源.
/// </summary>
public sealed class WebDavResource
{
    /// <summary>
    /// 初始化 <see cref="WebDavResource"/> 类的新实例.
    /// </summary>
    internal WebDavResource()
    {
        Properties = [];
        PropertyStatuses = [];
        ActiveLocks = [];
    }

    /// <summary>
    /// 获取资源 URI.
    /// </summary>
    public string Uri { get; internal set; } = string.Empty;

    /// <summary>
    /// 获取显示名称.
    /// </summary>
    public string? DisplayName { get; internal set; }

    /// <summary>
    /// 获取一个值，指示资源是否为集合（文件夹）.
    /// </summary>
    public bool IsCollection { get; internal set; }

    /// <summary>
    /// 获取一个值，指示资源是否隐藏.
    /// </summary>
    public bool IsHidden { get; internal set; }

    /// <summary>
    /// 获取内容长度（字节）.
    /// </summary>
    public long? ContentLength { get; internal set; }

    /// <summary>
    /// 获取内容类型.
    /// </summary>
    public string? ContentType { get; internal set; }

    /// <summary>
    /// 获取内容语言.
    /// </summary>
    public string? ContentLanguage { get; internal set; }

    /// <summary>
    /// 获取创建日期.
    /// </summary>
    public DateTimeOffset? CreationDate { get; internal set; }

    /// <summary>
    /// 获取最后修改日期.
    /// </summary>
    public DateTimeOffset? LastModifiedDate { get; internal set; }

    /// <summary>
    /// 获取 ETag.
    /// </summary>
    public string? ETag { get; internal set; }

    /// <summary>
    /// 获取资源的所有属性.
    /// </summary>
    public IReadOnlyCollection<WebDavProperty> Properties { get; internal set; }

    /// <summary>
    /// 获取属性状态集合.
    /// </summary>
    public IReadOnlyCollection<WebDavPropertyStatus> PropertyStatuses { get; internal set; }

    /// <summary>
    /// 获取活动锁集合.
    /// </summary>
    public IReadOnlyCollection<ActiveLock> ActiveLocks { get; internal set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is WebDavResource resource && Uri == resource.Uri;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Uri);

    /// <inheritdoc/>
    public override string ToString() => $"{(IsCollection ? "[Dir]" : "[File]")} {DisplayName ?? Uri}";
}

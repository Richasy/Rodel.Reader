// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Abstractions;

/// <summary>
/// 插件元数据，描述插件的基本信息.
/// </summary>
public sealed record PluginMetadata
{
    /// <summary>
    /// 插件唯一标识符.
    /// 建议使用反向域名格式，如 "com.example.myplugin".
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 插件显示名称.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 插件版本.
    /// 建议遵循语义化版本规范.
    /// </summary>
    public required Version Version { get; init; }

    /// <summary>
    /// 插件作者.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// 插件描述.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 插件主页链接.
    /// </summary>
    public string? Homepage { get; init; }

    /// <summary>
    /// 插件要求的最低宿主版本.
    /// </summary>
    public Version? MinHostVersion { get; init; }

    /// <summary>
    /// 插件图标 URL 或 Base64 编码的图标数据.
    /// </summary>
    public string? IconUri { get; init; }

    /// <summary>
    /// 插件支持的语言/区域列表.
    /// 空列表表示支持所有语言.
    /// </summary>
    public IReadOnlyList<string>? SupportedCultures { get; init; }

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc/>
    public bool Equals(PluginMetadata? other)
        => other is not null && Id.Equals(other.Id, StringComparison.Ordinal);
}

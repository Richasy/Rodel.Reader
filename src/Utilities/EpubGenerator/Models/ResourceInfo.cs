// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 资源文件信息.
/// </summary>
public sealed class ResourceInfo
{
    /// <summary>
    /// 资源唯一标识符.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 文件名（不含路径）.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// 文件数据.
    /// </summary>
    public required ReadOnlyMemory<byte> Data { get; init; }

    /// <summary>
    /// MIME 媒体类型.
    /// </summary>
    public required string MediaType { get; init; }

    /// <summary>
    /// 资源类型.
    /// </summary>
    public ResourceType Type { get; init; } = ResourceType.Image;

    /// <summary>
    /// 从字节数组创建资源信息.
    /// </summary>
    public static ResourceInfo FromBytes(string id, string fileName, byte[] data, string mediaType, ResourceType type = ResourceType.Image)
        => new() { Id = id, FileName = fileName, Data = data, MediaType = mediaType, Type = type };

    /// <summary>
    /// 从 ReadOnlyMemory 创建资源信息.
    /// </summary>
    public static ResourceInfo FromMemory(string id, string fileName, ReadOnlyMemory<byte> data, string mediaType, ResourceType type = ResourceType.Image)
        => new() { Id = id, FileName = fileName, Data = data, MediaType = mediaType, Type = type };
}

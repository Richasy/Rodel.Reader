// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.Fb2Parser;

/// <summary>
/// 表示 FB2 文件中的二进制资源。
/// </summary>
public sealed class Fb2Binary
{
    /// <summary>
    /// 获取或设置资源的 ID（用于引用）。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置资源的媒体类型。
    /// </summary>
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置 Base64 编码的数据。
    /// </summary>
    public string Base64Data { get; set; } = string.Empty;

    /// <summary>
    /// 获取一个值，指示此资源是否为图片。
    /// </summary>
    public bool IsImage => MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 获取解码后的字节数据。
    /// </summary>
    /// <returns>字节数组。</returns>
    public byte[] GetBytes()
    {
        if (string.IsNullOrEmpty(Base64Data))
        {
            return [];
        }

        try
        {
            return Convert.FromBase64String(Base64Data);
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// 获取数据大小（字节）。
    /// </summary>
    public int Size => string.IsNullOrEmpty(Base64Data) ? 0 : (Base64Data.Length * 3) / 4;

    /// <inheritdoc/>
    public override string ToString() => $"{Id} ({MediaType}, ~{Size} bytes)";
}

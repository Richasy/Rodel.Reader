// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Utilities.Fb2Parser;

/// <summary>
/// FB2 binary 节点解析器。
/// </summary>
internal static class BinaryParser
{

    /// <summary>
    /// 解析所有 binary 元素。
    /// </summary>
    /// <param name="binaryElements">binary 元素列表。</param>
    /// <returns>二进制资源列表。</returns>
    public static List<Fb2Binary> Parse(IEnumerable<XElement> binaryElements)
    {
        var binaries = new List<Fb2Binary>();

        foreach (var element in binaryElements)
        {
            var binary = ParseBinary(element);
            if (binary != null)
            {
                binaries.Add(binary);
            }
        }

        return binaries;
    }

    private static Fb2Binary? ParseBinary(XElement element)
    {
        var id = element.Attribute("id")?.Value;
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        var contentType = element.Attribute("content-type")?.Value ?? "application/octet-stream";
        var base64Data = element.Value.Trim();

        // 移除可能的空白字符
        base64Data = base64Data.Replace("\n", string.Empty, StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("\t", string.Empty, StringComparison.Ordinal);

        return new Fb2Binary
        {
            Id = id,
            MediaType = NormalizeMediaType(contentType),
            Base64Data = base64Data,
        };
    }

    private static string NormalizeMediaType(string contentType)
    {
        // 标准化媒体类型
        return contentType.ToLowerInvariant() switch
        {
            "image/jpg" => "image/jpeg",
            "jpg" => "image/jpeg",
            "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "bmp" => "image/bmp",
            "webp" => "image/webp",
            _ => contentType,
        };
    }
}

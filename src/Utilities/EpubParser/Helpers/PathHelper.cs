// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// EPUB 内容的路径工具类。
/// </summary>
internal static class PathHelper
{
    /// <summary>
    /// 从文件路径获取目录路径。
    /// </summary>
    public static string GetDirectoryPath(string filePath)
    {
        var lastSlashIndex = filePath.LastIndexOf('/');
        return lastSlashIndex >= 0 ? filePath[..lastSlashIndex] : string.Empty;
    }

    /// <summary>
    /// 将基础路径与相对路径组合。
    /// </summary>
    public static string Combine(string basePath, string relativePath)
    {
        if (string.IsNullOrEmpty(basePath))
        {
            return NormalizePath(relativePath);
        }

        if (string.IsNullOrEmpty(relativePath))
        {
            return NormalizePath(basePath);
        }

        // 处理绝对路径
        if (relativePath.StartsWith('/'))
        {
            return NormalizePath(relativePath[1..]);
        }

        var combined = basePath.TrimEnd('/') + "/" + relativePath;
        return NormalizePath(combined);
    }

    /// <summary>
    /// 通过解析 .. 和 . 段来规范化路径。
    /// </summary>
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        // 将反斜杠替换为正斜杠
        path = path.Replace('\\', '/');

        // 解码 URL 编码
        path = Uri.UnescapeDataString(path);

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>();

        foreach (var segment in segments)
        {
            if (segment == "..")
            {
                if (result.Count > 0)
                {
                    result.RemoveAt(result.Count - 1);
                }
            }
            else if (segment != ".")
            {
                result.Add(segment);
            }
        }

        return string.Join("/", result);
    }

    /// <summary>
    /// 将路径拆分为内容路径和锚点。
    /// </summary>
    public static (string Path, string? Anchor) SplitAnchor(string href)
    {
        var anchorIndex = href.IndexOf('#', StringComparison.Ordinal);
        if (anchorIndex >= 0)
        {
            return (href[..anchorIndex], href[(anchorIndex + 1)..]);
        }
        return (href, null);
    }

    /// <summary>
    /// 检查路径是否为远程 URL。
    /// </summary>
    public static bool IsRemoteUrl(string path)
    {
        return path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }
}

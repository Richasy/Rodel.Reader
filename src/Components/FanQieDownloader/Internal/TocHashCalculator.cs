// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Internal;

/// <summary>
/// 目录哈希计算器.
/// </summary>
internal static class TocHashCalculator
{
    /// <summary>
    /// 计算目录哈希，用于检测目录变化.
    /// </summary>
    /// <param name="volumes">卷列表.</param>
    /// <returns>目录哈希（16 位十六进制）.</returns>
    public static string Calculate(IReadOnlyList<BookVolume> volumes)
    {
        // 将所有章节 ID 按顺序拼接，计算 SHA256
        var allChapterIds = volumes
            .SelectMany(v => v.Chapters)
            .OrderBy(c => c.Order)
            .Select(c => c.ItemId);

        var combined = string.Join("|", allChapterIds);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hash)[..16]; // 取前 16 位
    }

    /// <summary>
    /// 计算目录哈希.
    /// </summary>
    /// <param name="chapterIds">有序的章节 ID 列表.</param>
    /// <returns>目录哈希（16 位十六进制）.</returns>
    public static string Calculate(IEnumerable<string> chapterIds)
    {
        var combined = string.Join("|", chapterIds);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hash)[..16];
    }
}

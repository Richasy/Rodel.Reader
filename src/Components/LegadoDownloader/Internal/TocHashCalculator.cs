// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.Legado.Internal;

/// <summary>
/// 目录哈希计算器.
/// </summary>
internal static class TocHashCalculator
{
    /// <summary>
    /// 计算目录哈希，用于检测目录变化.
    /// </summary>
    /// <param name="chapters">章节列表.</param>
    /// <returns>目录哈希（16 位十六进制）.</returns>
    public static string Calculate(IReadOnlyList<Chapter> chapters)
    {
        // 将所有章节 URL 按索引顺序拼接，计算 SHA256
        var allChapterUrls = chapters
            .OrderBy(c => c.Index)
            .Select(c => c.Url);

        return Calculate(allChapterUrls);
    }

    /// <summary>
    /// 计算目录哈希.
    /// </summary>
    /// <param name="chapterUrls">有序的章节 URL 列表.</param>
    /// <returns>目录哈希（16 位十六进制）.</returns>
    public static string Calculate(IEnumerable<string> chapterUrls)
    {
        var combined = string.Join("|", chapterUrls);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hash)[..16];
    }
}

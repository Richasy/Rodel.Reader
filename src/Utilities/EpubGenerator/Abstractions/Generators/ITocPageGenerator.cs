// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 可视化目录页生成器.
/// </summary>
public interface ITocPageGenerator
{
    /// <summary>
    /// 生成可视化目录页 XHTML 内容.
    /// </summary>
    /// <param name="chapters">章节列表.</param>
    /// <param name="title">目录标题（可选，默认为"目录"）.</param>
    /// <returns>目录页的 XHTML 字符串.</returns>
    string Generate(IReadOnlyList<ChapterInfo> chapters, string? title = null);
}

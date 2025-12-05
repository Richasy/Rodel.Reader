// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 章节页面生成器.
/// </summary>
public interface IChapterGenerator
{
    /// <summary>
    /// 生成章节页 XHTML 内容.
    /// </summary>
    /// <param name="chapter">章节信息.</param>
    /// <returns>章节页的 XHTML 字符串.</returns>
    string Generate(ChapterInfo chapter);
}

// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// Nav 文档生成器 (EPUB 3).
/// </summary>
public interface INavDocGenerator
{
    /// <summary>
    /// 生成 nav.xhtml 内容.
    /// </summary>
    /// <param name="metadata">书籍元数据.</param>
    /// <param name="chapters">章节列表.</param>
    /// <returns>nav.xhtml 的 XHTML 字符串.</returns>
    string Generate(EpubMetadata metadata, IReadOnlyList<ChapterInfo> chapters);
}

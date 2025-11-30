// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// NCX 导航文件生成器 (EPUB 2).
/// </summary>
public interface INcxGenerator
{
    /// <summary>
    /// 生成 toc.ncx 内容.
    /// </summary>
    /// <param name="metadata">书籍元数据.</param>
    /// <param name="chapters">章节列表.</param>
    /// <returns>toc.ncx 的 XML 字符串.</returns>
    string Generate(EpubMetadata metadata, IReadOnlyList<ChapterInfo> chapters);
}

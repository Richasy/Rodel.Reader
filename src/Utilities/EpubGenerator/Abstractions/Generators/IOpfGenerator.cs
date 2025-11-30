// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// OPF (Open Packaging Format) 文件生成器.
/// </summary>
public interface IOpfGenerator
{
    /// <summary>
    /// 生成 content.opf 内容.
    /// </summary>
    /// <param name="metadata">书籍元数据.</param>
    /// <param name="chapters">章节列表.</param>
    /// <param name="options">生成选项（可选）.</param>
    /// <returns>content.opf 的 XML 字符串.</returns>
    string Generate(EpubMetadata metadata, IReadOnlyList<ChapterInfo> chapters, EpubOptions? options = null);
}

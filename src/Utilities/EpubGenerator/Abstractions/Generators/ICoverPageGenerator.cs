// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 封面页生成器.
/// </summary>
public interface ICoverPageGenerator
{
    /// <summary>
    /// 生成封面页 XHTML 内容.
    /// </summary>
    /// <param name="cover">封面信息.</param>
    /// <param name="title">书籍标题（用于 alt 文本）.</param>
    /// <returns>封面页的 XHTML 字符串.</returns>
    string Generate(CoverInfo cover, string title);
}

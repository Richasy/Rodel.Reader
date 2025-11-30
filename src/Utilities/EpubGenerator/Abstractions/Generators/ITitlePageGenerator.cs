// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 标题页生成器.
/// </summary>
public interface ITitlePageGenerator
{
    /// <summary>
    /// 生成标题页 XHTML 内容.
    /// </summary>
    /// <param name="metadata">书籍元数据.</param>
    /// <returns>标题页的 XHTML 字符串.</returns>
    string Generate(EpubMetadata metadata);
}

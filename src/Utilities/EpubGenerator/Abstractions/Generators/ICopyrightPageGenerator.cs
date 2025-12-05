// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 版权页生成器.
/// </summary>
public interface ICopyrightPageGenerator
{
    /// <summary>
    /// 生成版权页 XHTML 内容.
    /// </summary>
    /// <param name="metadata">书籍元数据.</param>
    /// <returns>版权页的 XHTML 字符串.</returns>
    string Generate(EpubMetadata metadata);
}

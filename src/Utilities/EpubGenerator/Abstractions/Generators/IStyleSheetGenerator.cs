// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 样式表生成器.
/// </summary>
public interface IStyleSheetGenerator
{
    /// <summary>
    /// 生成样式表 CSS 内容.
    /// </summary>
    /// <param name="options">生成选项（可选，用于追加自定义 CSS）.</param>
    /// <returns>CSS 样式表字符串.</returns>
    string Generate(EpubOptions? options = null);
}

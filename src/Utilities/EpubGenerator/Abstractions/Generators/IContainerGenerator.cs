// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// Container.xml 生成器.
/// </summary>
public interface IContainerGenerator
{
    /// <summary>
    /// 生成 container.xml 内容.
    /// </summary>
    /// <returns>container.xml 的 XML 字符串.</returns>
    string Generate();
}

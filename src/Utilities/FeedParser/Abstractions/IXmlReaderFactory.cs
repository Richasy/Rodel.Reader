// Copyright (c) Reader Copilot. All rights reserved.

using System.Xml;

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// XML 读取器工厂接口.
/// </summary>
/// <remarks>
/// 用于创建 XmlReader 实例，便于测试时进行 mock.
/// </remarks>
public interface IXmlReaderFactory
{
    /// <summary>
    /// 从流创建 XmlReader.
    /// </summary>
    /// <param name="stream">输入流.</param>
    /// <param name="async">是否启用异步模式.</param>
    /// <returns>XmlReader 实例.</returns>
    XmlReader CreateReader(Stream stream, bool async = true);

    /// <summary>
    /// 从字符串创建 XmlReader.
    /// </summary>
    /// <param name="xml">XML 字符串.</param>
    /// <param name="async">是否启用异步模式.</param>
    /// <returns>XmlReader 实例.</returns>
    XmlReader CreateReader(string xml, bool async = false);

    /// <summary>
    /// 从 TextReader 创建 XmlReader.
    /// </summary>
    /// <param name="textReader">文本读取器.</param>
    /// <param name="async">是否启用异步模式.</param>
    /// <returns>XmlReader 实例.</returns>
    XmlReader CreateReader(TextReader textReader, bool async = true);
}

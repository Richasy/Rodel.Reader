// Copyright (c) Reader Copilot. All rights reserved.

using System.Xml;

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// XML 写入器工厂接口.
/// </summary>
/// <remarks>
/// 用于创建 XmlWriter 实例，便于测试时进行 mock.
/// </remarks>
public interface IXmlWriterFactory
{
    /// <summary>
    /// 从流创建 XmlWriter.
    /// </summary>
    /// <param name="stream">输出流.</param>
    /// <param name="async">是否启用异步模式.</param>
    /// <returns>XmlWriter 实例.</returns>
    XmlWriter CreateWriter(Stream stream, bool async = true);

    /// <summary>
    /// 从 TextWriter 创建 XmlWriter.
    /// </summary>
    /// <param name="textWriter">文本写入器.</param>
    /// <param name="async">是否启用异步模式.</param>
    /// <returns>XmlWriter 实例.</returns>
    XmlWriter CreateWriter(TextWriter textWriter, bool async = true);

    /// <summary>
    /// 从 StringBuilder 创建 XmlWriter.
    /// </summary>
    /// <param name="builder">StringBuilder 实例.</param>
    /// <param name="async">是否启用异步模式.</param>
    /// <returns>XmlWriter 实例.</returns>
    XmlWriter CreateWriter(System.Text.StringBuilder builder, bool async = false);
}

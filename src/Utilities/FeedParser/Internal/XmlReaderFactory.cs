// Copyright (c) Reader Copilot. All rights reserved.

using System.Xml;

namespace Richasy.RodelReader.Utilities.FeedParser.Internal;

/// <summary>
/// XML 读取器工厂实现.
/// </summary>
internal sealed class XmlReaderFactory : IXmlReaderFactory
{
    /// <inheritdoc/>
    public XmlReader CreateReader(Stream stream, bool async = true)
    {
        var settings = CreateSettings(async);
        // 使用包装流来跳过前导空白字符
        var wrappedStream = new LeadingWhitespaceSkippingStream(stream);
        return XmlReader.Create(wrappedStream, settings);
    }

    /// <inheritdoc/>
    public XmlReader CreateReader(string xml, bool async = false)
    {
        var settings = CreateSettings(async);
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        return XmlReader.Create(new StringReader(xml), settings);
    }

    /// <inheritdoc/>
    public XmlReader CreateReader(TextReader textReader, bool async = true)
    {
        var settings = CreateSettings(async);
        return XmlReader.Create(textReader, settings);
    }

    private static XmlReaderSettings CreateSettings(bool async)
    {
        return new XmlReaderSettings
        {
            Async = async,
            DtdProcessing = DtdProcessing.Ignore,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true,
        };
    }
}

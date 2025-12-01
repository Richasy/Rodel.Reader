// Copyright (c) Reader Copilot. All rights reserved.

using System.Text;
using System.Xml;

namespace Richasy.RodelPlayer.Utilities.FeedParser.Internal;

/// <summary>
/// XML 写入器工厂实现.
/// </summary>
internal sealed class XmlWriterFactory : IXmlWriterFactory
{
    /// <inheritdoc/>
    public XmlWriter CreateWriter(Stream stream, bool async = true)
    {
        var settings = CreateSettings(async);
        return XmlWriter.Create(stream, settings);
    }

    /// <inheritdoc/>
    public XmlWriter CreateWriter(TextWriter textWriter, bool async = true)
    {
        var settings = CreateSettings(async);
        return XmlWriter.Create(textWriter, settings);
    }

    /// <inheritdoc/>
    public XmlWriter CreateWriter(StringBuilder builder, bool async = false)
    {
        var settings = CreateSettings(async);
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        return XmlWriter.Create(builder, settings);
    }

    private static XmlWriterSettings CreateSettings(bool async)
    {
        return new XmlWriterSettings
        {
            Async = async,
            Encoding = Encoding.UTF8,
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = false,
        };
    }
}

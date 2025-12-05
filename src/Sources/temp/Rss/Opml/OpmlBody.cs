// Copyright (c) Reader Copilot. All rights reserved.

using System.Text;
using System.Xml;

namespace Richasy.ReaderKernel.Models.Rss.Opml;

/// <summary>
/// OPML下的Body标签.
/// </summary>
public sealed class OpmlBody
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlBody"/> class.
    /// </summary>
    public OpmlBody()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlBody"/> class.
    /// </summary>
    public OpmlBody(XmlElement element)
    {
        if (element.Name.Equals("body", StringComparison.OrdinalIgnoreCase))
        {
            foreach (XmlNode node in element.ChildNodes)
            {
                if (node.Name.Equals("outline", StringComparison.OrdinalIgnoreCase))
                {
                    Outlines.Add(new OpmlOutline((XmlElement)node));
                }
            }
        }
    }

    /// <summary>
    /// Outline list.
    /// </summary>
    public List<OpmlOutline> Outlines { get; set; } = new List<OpmlOutline>();

    /// <inheritdoc/>
    public override string ToString()
    {
        var buf = new StringBuilder();
        _ = buf.Append("<body>\r\n");
        foreach (var outline in Outlines)
        {
            _ = buf.Append(outline.ToString());
        }

        _ = buf.Append("</body>\r\n");

        return buf.ToString();
    }
}

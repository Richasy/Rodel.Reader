// Copyright (c) Reader Copilot. All rights reserved.

using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Richasy.ReaderKernel.Models.Rss.Opml;

/// <summary>
/// OPML 配置.
/// </summary>
public sealed class OpmlConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlConfiguration"/> class.
    /// </summary>
    public OpmlConfiguration()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlConfiguration"/> class.
    /// </summary>
    /// <param name="content">OPML 文件路径.</param>
    public OpmlConfiguration(string content)
    {
        var regex = new Regex("(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");
        var matchs = regex.Matches(content);
        if (matchs.Count > 0)
        {
            foreach (var match in matchs)
            {
                content = content.Replace(match.ToString()!, WebUtility.HtmlEncode(match.ToString()), StringComparison.OrdinalIgnoreCase);
            }
        }

        var doc = new XmlDocument();
        doc.LoadXml(content);
        ReadOpmlNodes(doc);
    }

    /// <summary>
    /// Version of OPML.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Encoding of OPML.
    /// </summary>
    public string Encoding { get; set; }

    /// <summary>
    /// Head of OPML.
    /// </summary>
    public OpmlHead Head { get; set; } = new OpmlHead();

    /// <summary>
    /// Body of OPML.
    /// </summary>
    public OpmlBody Body { get; set; } = new OpmlBody();

    /// <inheritdoc/>
    public override string ToString()
    {
        var buf = new StringBuilder();
        var ecoding = string.IsNullOrEmpty(Encoding) ? "UTF-8" : Encoding;
        _ = buf.Append($"<?xml version=\"1.0\" encoding=\"{ecoding}\" ?>\r\n");
        var version = string.IsNullOrEmpty(Version) ? "2.0" : Version;
        _ = buf.Append($"<opml version=\"{version}\">\r\n");
        _ = buf.Append(Head.ToString());
        _ = buf.Append(Body.ToString());
        _ = buf.Append("</opml>");

        return buf.ToString();
    }

    private void ReadOpmlNodes(XmlDocument doc)
    {
        foreach (XmlNode nodes in doc)
        {
            if (nodes.Name.Equals("opml", StringComparison.OrdinalIgnoreCase))
            {
                foreach (XmlNode childNode in nodes)
                {
                    if (childNode.Name.Equals("head", StringComparison.OrdinalIgnoreCase))
                    {
                        Head = new OpmlHead((XmlElement)childNode);
                    }

                    if (childNode.Name.Equals("body", StringComparison.OrdinalIgnoreCase))
                    {
                        Body = new OpmlBody((XmlElement)childNode);
                    }
                }
            }
        }
    }
}

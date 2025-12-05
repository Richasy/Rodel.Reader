// Copyright (c) Reader Copilot. All rights reserved.

using System.Net;
using System.Text;
using System.Xml;

namespace Richasy.ReaderKernel.Models.Rss.Opml;

/// <summary>
/// 单个条目信息.
/// </summary>
public sealed class OpmlOutline
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlOutline"/> class.
    /// </summary>
    public OpmlOutline()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlOutline"/> class.
    /// </summary>
    public OpmlOutline(RssFeed feed)
    {
        Type = "rss";
        Text = feed.Name;
        Title = feed.Name;
        Description = feed.Description ?? string.Empty;
        XMLUrl = WebUtility.HtmlEncode(feed.Url);
        HTMLUrl = WebUtility.HtmlEncode(feed.Website);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlOutline"/> class.
    /// </summary>
    public OpmlOutline(XmlElement element)
    {
        Text = element.GetAttribute("text");
        IsComment = element.GetAttribute("isComment");
        IsBreakpoint = element.GetAttribute("isBreakpoint");
        Created = GetDateTimeAttribute(element, "created");
        Category = GetCategoriesAttribute(element, "category");
        Description = element.GetAttribute("description");
        HTMLUrl = element.GetAttribute("htmlUrl");
        Language = element.GetAttribute("language");
        Title = element.GetAttribute("title");
        Type = element.GetAttribute("type");
        Version = element.GetAttribute("version");
        XMLUrl = element.GetAttribute("xmlUrl");
        if (string.IsNullOrEmpty(Title))
        {
            Title = Text;
        }

        if (element.HasChildNodes)
        {
            foreach (XmlNode child in element.ChildNodes)
            {
                if (child.Name.Equals("outline", StringComparison.OrdinalIgnoreCase))
                {
                    Outlines.Add(new OpmlOutline((XmlElement)child));
                }
            }
        }
    }

    /// <summary>
    /// Text of the XML file (required).
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// true / false.
    /// </summary>
    public string IsComment { get; set; }

    /// <summary>
    /// true / false.
    /// </summary>
    public string IsBreakpoint { get; set; }

    /// <summary>
    /// outline node was created.
    /// </summary>
    public DateTime? Created { get; set; }

    /// <summary>
    /// Categories.
    /// </summary>
    public List<string> Category { get; set; } = [];

    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// HTML URL.
    /// </summary>
    public string? HTMLUrl { get; set; }

    /// <summary>
    /// Language.
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// Title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Type (rss/atom).
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Version of RSS.
    /// RSS1 for RSS1.0. RSS for 0.91, 0.92 or 2.0.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// URL of the XML file.
    /// </summary>
    public string XMLUrl { get; set; }

    /// <summary>
    /// Outline list.
    /// </summary>
    public List<OpmlOutline> Outlines { get; set; } = [];

    /// <inheritdoc/>
    public override string ToString()
    {
        var buf = new StringBuilder();
        _ = buf.Append("<outline");
        _ = buf.Append(GetAttributeString("text", Text));
        _ = buf.Append(GetAttributeString("isComment", IsComment));
        _ = buf.Append(GetAttributeString("isBreakpoint", IsBreakpoint));
        _ = buf.Append(GetAttributeString("created", Created));
        _ = buf.Append(GetAttributeString("category", Category));
        _ = buf.Append(GetAttributeString("description", Description ?? string.Empty));
        _ = buf.Append(GetAttributeString("htmlUrl", HTMLUrl ?? string.Empty));
        _ = buf.Append(GetAttributeString("language", Language));
        _ = buf.Append(GetAttributeString("title", Title));
        _ = buf.Append(GetAttributeString("type", Type));
        _ = buf.Append(GetAttributeString("version", Version));
        _ = buf.Append(GetAttributeString("xmlUrl", XMLUrl));

        if (Outlines.Count > 0)
        {
            _ = buf.Append(">\r\n");
            foreach (var outline in Outlines)
            {
                _ = buf.Append(outline.ToString());
            }

            _ = buf.Append("</outline>\r\n");
        }
        else
        {
            _ = buf.Append(" />\r\n");
        }

        return buf.ToString();
    }

    private static DateTime? GetDateTimeAttribute(XmlElement element, string name)
    {
        var dt = element.GetAttribute(name);

        try
        {
            return DateTime.Parse(dt);
        }
        catch
        {
            return null;
        }
    }

    private static List<string> GetCategoriesAttribute(XmlElement element, string name)
    {
        var list = new List<string>();
        var items = element.GetAttribute(name).Split(',');
        foreach (var item in items)
        {
            list.Add(item.Trim());
        }

        return list;
    }

    private static string GetAttributeString(string name, string value)
        => string.IsNullOrEmpty(value) ? string.Empty : $" {name}=\"{WebUtility.HtmlEncode(value)}\"";

    private static string GetAttributeString(string name, DateTime? value)
        => value == null ? string.Empty : $" {name}=\"{value.Value:R}\"";

    private static string GetAttributeString(string name, List<string> value)
    {
        if (value.Count == 0)
        {
            return string.Empty;
        }

        var buf = new StringBuilder();
        foreach (var item in value)
        {
            _ = buf.Append(item);
            _ = buf.Append(',');
        }

        return $" {name}=\"{buf.Remove(buf.Length - 1, 1)}\"";
    }
}

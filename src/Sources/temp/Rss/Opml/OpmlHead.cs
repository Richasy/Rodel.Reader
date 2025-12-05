// Copyright (c) Reader Copilot. All rights reserved.

using System.Text;
using System.Xml;

namespace Richasy.ReaderKernel.Models.Rss.Opml;

/// <summary>
/// 头部信息.
/// </summary>
public sealed class OpmlHead
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlHead"/> class.
    /// </summary>
    public OpmlHead()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlHead"/> class.
    /// </summary>
    public OpmlHead(XmlElement element)
    {
        if (element.Name.Equals("head", StringComparison.OrdinalIgnoreCase))
        {
            foreach (XmlNode node in element.ChildNodes)
            {
                Title = GetStringValue(node, "title", Title!);
                DateCreated = GetDateTimeValue(node, "dateCreated", DateCreated);
                DateModified = GetDateTimeValue(node, "dateModified", DateModified);
                OwnerName = GetStringValue(node, "ownerName", OwnerName!);
                OwnerEmail = GetStringValue(node, "ownerEmail", OwnerEmail!);
                OwnerId = GetStringValue(node, "ownerId", OwnerId!);
                Docs = GetStringValue(node, "docs", Docs!);
                ExpansionState = GetExpansionState(node, "expansionState", ExpansionState);
                VertScrollState = GetStringValue(node, "vertScrollState", VertScrollState!);
                WindowTop = GetStringValue(node, "windowTop", WindowTop!);
                WindowLeft = GetStringValue(node, "windowLeft", WindowLeft!);
                WindowBottom = GetStringValue(node, "windowBottom", WindowBottom!);
                WindowRight = GetStringValue(node, "windowRight", WindowRight!);
            }
        }
    }

    /// <summary>
    /// title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Created date.
    /// </summary>
    public DateTime? DateCreated { get; set; }

    /// <summary>
    /// Modified date.
    /// </summary>
    public DateTime? DateModified { get; set; }

    /// <summary>
    /// Owner Name.
    /// </summary>
    public string OwnerName { get; set; }

    /// <summary>
    /// Owner Email.
    /// </summary>
    public string OwnerEmail { get; set; }

    /// <summary>
    /// Owner Id.
    /// </summary>
    public string OwnerId { get; set; }

    /// <summary>
    /// Docs.
    /// </summary>
    public string Docs { get; set; }

    /// <summary>
    /// expansionState.
    /// </summary>
    public List<string> ExpansionState { get; set; } = new List<string>();

    /// <summary>
    /// vertScrollState.
    /// </summary>
    public string VertScrollState { get; set; }

    /// <summary>
    /// windowTop.
    /// </summary>
    public string WindowTop { get; set; }

    /// <summary>
    /// windowLeft.
    /// </summary>
    public string WindowLeft { get; set; }

    /// <summary>
    /// windowBottom.
    /// </summary>
    public string WindowBottom { get; set; }

    /// <summary>
    /// windowRight.
    /// </summary>
    public string WindowRight { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        var buf = new StringBuilder();
        _ = buf.Append("<head>\r\n");
        _ = buf.Append(GetNodeString("title", Title));
        _ = buf.Append(GetNodeString("dateCreated", DateCreated));
        _ = buf.Append(GetNodeString("dateModified", DateModified));
        _ = buf.Append(GetNodeString("ownerName", OwnerName));
        _ = buf.Append(GetNodeString("ownerEmail", OwnerEmail));
        _ = buf.Append(GetNodeString("ownerId", OwnerId));
        _ = buf.Append(GetNodeString("docs", Docs));
        _ = buf.Append(GetNodeString("expansionState", ExpansionState));
        _ = buf.Append(GetNodeString("vertScrollState", VertScrollState));
        _ = buf.Append(GetNodeString("windowTop", WindowTop));
        _ = buf.Append(GetNodeString("windowLeft", WindowLeft));
        _ = buf.Append(GetNodeString("windowBottom", WindowBottom));
        _ = buf.Append(GetNodeString("windowRight", WindowRight));
        _ = buf.Append("</head>\r\n");
        return buf.ToString();
    }

    private static string GetStringValue(XmlNode node, string name, string value)
    {
        return node.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            ? node.InnerText
            : !node.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ? value : string.Empty;
    }

    private static DateTime? GetDateTimeValue(XmlNode node, string name, DateTime? value)
    {
        if (node.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return DateTime.Parse(node.InnerText);
            }
            catch
            {
                return null;
            }
        }
        else
        {
            return !node.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ? value : null;
        }
    }

    private static List<string> GetExpansionState(XmlNode node, string name, List<string> value)
    {
        if (node.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            var list = new List<string>();
            var items = node.InnerText.Split(',');
            foreach (var item in items)
            {
                list.Add(item.Trim());
            }

            return list;
        }
        else
        {
            return !node.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ? value : [];
        }
    }

    private static string GetNodeString(string name, string value)
        => string.IsNullOrEmpty(value) ? string.Empty : $"<{name}>{value}</{name}>\r\n";

    private static string GetNodeString(string name, DateTime? value)
        => value == null ? string.Empty : $"<{name}>{value.Value:R}</{name}>\r\n";

    private static string GetNodeString(string name, List<string> value)
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

        return $"<{name}>{buf.Remove(buf.Length - 1, 1)}</{name}>\r\n";
    }
}

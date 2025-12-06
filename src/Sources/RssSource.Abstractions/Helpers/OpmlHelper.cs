// Copyright (c) Richasy. All rights reserved.

using System.Net;
using System.Text;
using System.Xml;
using Richasy.RodelReader.Sources.Rss.Abstractions.Helpers;

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// OPML 解析和生成辅助类.
/// </summary>
public static class OpmlHelper
{
    /// <summary>
    /// 从 OPML 内容解析出分组和订阅源列表.
    /// </summary>
    /// <param name="opmlContent">OPML 文件内容.</param>
    /// <returns>分组列表和订阅源列表的元组.</returns>
    public static (List<RssFeedGroup> Groups, List<RssFeed> Feeds) ParseOpml(string opmlContent)
    {
        Guard.NotNullOrWhiteSpace(opmlContent, nameof(opmlContent));

        var groups = new List<RssFeedGroup>();
        var feeds = new List<RssFeed>();

        var doc = new XmlDocument();
        doc.LoadXml(EncodeUrls(opmlContent));

        var bodyNode = doc.SelectSingleNode("//body");
        if (bodyNode == null)
        {
            return (groups, feeds);
        }

        foreach (XmlNode outline in bodyNode.ChildNodes)
        {
            if (outline.Name.Equals("outline", StringComparison.OrdinalIgnoreCase))
            {
                ParseOutline((XmlElement)outline, groups, feeds, groupId: null);
            }
        }

        return (groups, feeds);
    }

    /// <summary>
    /// 生成 OPML 内容.
    /// </summary>
    /// <param name="groups">分组列表.</param>
    /// <param name="feeds">订阅源列表.</param>
    /// <param name="title">OPML 标题.</param>
    /// <returns>OPML 文件内容.</returns>
    public static string GenerateOpml(
        IEnumerable<RssFeedGroup> groups,
        IEnumerable<RssFeed> feeds,
        string title = "RSS Subscriptions")
    {
        var feedList = feeds.ToList();
        var groupList = groups.ToList();

        var buf = new StringBuilder();
        buf.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
        buf.AppendLine("<opml version=\"2.0\">");

        // Head
        buf.AppendLine("<head>");
        buf.AppendLine($"<title>{WebUtility.HtmlEncode(title)}</title>");
        buf.AppendLine($"<dateCreated>{DateTime.UtcNow:R}</dateCreated>");
        buf.AppendLine("</head>");

        // Body
        buf.AppendLine("<body>");

        // 按分组输出订阅源
        foreach (var group in groupList)
        {
            var groupFeeds = feedList.Where(f => f.GroupIds?.Contains(group.Id, StringComparison.Ordinal) == true).ToList();
            if (groupFeeds.Count == 0)
            {
                continue;
            }

            buf.AppendLine($"<outline text=\"{WebUtility.HtmlEncode(group.Name)}\" title=\"{WebUtility.HtmlEncode(group.Name)}\">");
            foreach (var feed in groupFeeds)
            {
                buf.AppendLine(GenerateFeedOutline(feed));
            }

            buf.AppendLine("</outline>");
        }

        // 输出未分组的订阅源
        var ungroupedFeeds = feedList.Where(f => string.IsNullOrEmpty(f.GroupIds)).ToList();
        foreach (var feed in ungroupedFeeds)
        {
            buf.AppendLine(GenerateFeedOutline(feed));
        }

        buf.AppendLine("</body>");
        buf.AppendLine("</opml>");

        return buf.ToString();
    }

    private static void ParseOutline(
        XmlElement element,
        List<RssFeedGroup> groups,
        List<RssFeed> feeds,
        string? groupId)
    {
        var xmlUrl = element.GetAttribute("xmlUrl");
        var hasChildren = element.HasChildNodes &&
                          element.ChildNodes.Cast<XmlNode>().Any(n => n.Name.Equals("outline", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(xmlUrl))
        {
            // 这是一个订阅源
            var feed = ParseFeedFromOutline(element, groupId);
            var existingFeed = feeds.FirstOrDefault(f => f.Url == feed.Url);
            if (existingFeed != null)
            {
                // 订阅源已存在，添加分组
                if (!string.IsNullOrEmpty(groupId) && existingFeed.GroupIds?.Contains(groupId, StringComparison.Ordinal) != true)
                {
                    existingFeed.GroupIds = string.IsNullOrEmpty(existingFeed.GroupIds)
                        ? groupId
                        : $"{existingFeed.GroupIds},{groupId}";
                }
            }
            else
            {
                feeds.Add(feed);
            }
        }
        else if (hasChildren)
        {
            // 这是一个分组
            var title = element.GetAttribute("title");
            if (string.IsNullOrEmpty(title))
            {
                title = element.GetAttribute("text");
            }

            var group = new RssFeedGroup
            {
                Id = $"folder/{title}",
                Name = title,
            };
            groups.Add(group);

            // 递归处理子节点
            foreach (XmlNode child in element.ChildNodes)
            {
                if (child.Name.Equals("outline", StringComparison.OrdinalIgnoreCase))
                {
                    ParseOutline((XmlElement)child, groups, feeds, group.Id);
                }
            }
        }
    }

    private static RssFeed ParseFeedFromOutline(XmlElement element, string? groupId)
    {
        var title = element.GetAttribute("title");
        if (string.IsNullOrEmpty(title))
        {
            title = element.GetAttribute("text");
        }

        var xmlUrl = element.GetAttribute("xmlUrl");
        var htmlUrl = element.GetAttribute("htmlUrl");
        var description = element.GetAttribute("description");

        return new RssFeed
        {
            Id = $"feed/{xmlUrl}",
            Name = WebUtility.HtmlDecode(title),
            Url = WebUtility.HtmlDecode(xmlUrl),
            Website = WebUtility.HtmlDecode(htmlUrl),
            Description = WebUtility.HtmlDecode(description),
            GroupIds = groupId,
        };
    }

    private static string GenerateFeedOutline(RssFeed feed)
    {
        var buf = new StringBuilder();
        buf.Append("<outline type=\"rss\"");
        buf.Append($" text=\"{WebUtility.HtmlEncode(feed.Name)}\"");
        buf.Append($" title=\"{WebUtility.HtmlEncode(feed.Name)}\"");

        if (!string.IsNullOrEmpty(feed.Description))
        {
            buf.Append($" description=\"{WebUtility.HtmlEncode(feed.Description)}\"");
        }

        buf.Append($" xmlUrl=\"{WebUtility.HtmlEncode(feed.Url)}\"");

        if (!string.IsNullOrEmpty(feed.Website))
        {
            buf.Append($" htmlUrl=\"{WebUtility.HtmlEncode(feed.Website)}\"");
        }

        buf.Append(" />");
        return buf.ToString();
    }

    private static string EncodeUrls(string content)
    {
        // 对 OPML 中的 URL 进行编码，避免 XML 解析错误
        var regex = new System.Text.RegularExpressions.Regex(
            "(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");
        var matches = regex.Matches(content);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var url = match.Value;
            var encodedUrl = WebUtility.HtmlEncode(url);
            if (url != encodedUrl)
            {
                content = content.Replace(url, encodedUrl, StringComparison.Ordinal);
            }
        }

        return content;
    }
}

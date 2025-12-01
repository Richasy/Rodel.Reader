// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser.Internal;

/// <summary>
/// RSS 元素名称定义.
/// </summary>
internal static class RssElementNames
{
    // 根元素
    public const string Rss = "rss";
    public const string Channel = "channel";
    public const string Item = "item";

    // 频道元素
    public const string Title = "title";
    public const string Link = "link";
    public const string Description = "description";
    public const string Language = "language";
    public const string Copyright = "copyright";
    public const string ManagingEditor = "managingEditor";
    public const string WebMaster = "webMaster";
    public const string PubDate = "pubDate";
    public const string LastBuildDate = "lastBuildDate";
    public const string Category = "category";
    public const string Generator = "generator";
    public const string Docs = "docs";
    public const string Cloud = "cloud";
    public const string Ttl = "ttl";
    public const string Image = "image";

    // 图片子元素
    public const string Url = "url";
    public const string Width = "width";
    public const string Height = "height";

    // 订阅项元素
    public const string Author = "author";
    public const string Comments = "comments";
    public const string Enclosure = "enclosure";
    public const string Guid = "guid";
    public const string Source = "source";

    // content:encoded
    public const string Encoded = "encoded";

    // 属性
    public const string Version = "version";
    public const string IsPermaLink = "isPermaLink";
    public const string Type = "type";
    public const string Length = "length";
    public const string Href = "href";
    public const string Rel = "rel";

    // iTunes 播客元素
    public const string ITunesAuthor = "author";
    public const string ITunesSummary = "summary";
    public const string ITunesDuration = "duration";
    public const string ITunesImage = "image";
    public const string ITunesExplicit = "explicit";
    public const string ITunesCategory = "category";
    public const string ITunesOwner = "owner";
    public const string ITunesName = "name";
    public const string ITunesEmail = "email";
}

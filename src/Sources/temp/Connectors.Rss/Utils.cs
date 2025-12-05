// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.ReaderKernel.Models.Config;
using Richasy.ReaderKernel.Models.Rss;
using Richasy.ReaderKernel.Toolkits;
using Richasy.ReaderKernel.Toolkits.Feed;
using Richasy.ReaderKernel.Toolkits.Feed.Atom;
using Richasy.ReaderKernel.Toolkits.Feed.Rss;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace Richasy.ReaderKernel.Connectors.Rss;

public static class Utils
{
    private const string DefaultAcceptString = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
    private const string DefaultUserAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36 Edg/92.0.902.62";

    public static CookieContainer CookieContainer { get; private set; }

    public static HttpClient GetHttpClient(bool allowCookie = false)
    {
        var handler = new HttpClientHandler()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        };

        if (allowCookie)
        {
            CookieContainer = new CookieContainer();
            handler.CookieContainer = CookieContainer;
            handler.UseCookies = true;
        }

        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };
        client.DefaultRequestHeaders.Add("accept", DefaultAcceptString);
        client.DefaultRequestHeaders.Add("user-agent", DefaultUserAgentString);
        return client;
    }

    public static async Task<RssFeedDetail?> GetFeedDetailAsync(this HttpClient httpClient, string feedUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetByteArrayAsync(new Uri(feedUrl), cancellationToken).ConfigureAwait(false);
            var content = Encoding.Default.GetString(response);
            content = RemoveBom(content);
            using var stringReader = new StringReader(content);
            var xdoc = XDocument.Load(stringReader);
            var encoding = xdoc.Declaration?.Encoding;
            if (!string.IsNullOrEmpty(encoding))
            {
                content = Encoding.GetEncoding(encoding).GetString(response);
                content = RemoveBom(content);
            }

            var feedType = GetFeedType(content, out var xmlReader);
            if (feedType == "unknown")
            {
                return default;
            }

            var reader = feedType == "rss" ? (XmlFeedReader)new RssFeedReader(xmlReader) : new AtomFeedReader(xmlReader);
            var items = new List<RssArticleBase>();
            var feed = new RssFeed();
            while (await reader.Read().ConfigureAwait(false))
            {
                switch (reader.ElementType)
                {
                    case SyndicationElementType.Item:
                        var item = await reader.ReadItem().ConfigureAwait(false);
                        var article = ConvertSyndicationItemToArticle(item);
                        items.Add(article);
                        break;
                    case SyndicationElementType.Link:
                        var url = await reader.ReadLink().ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(url?.Uri?.ToString()))
                        {
                            feed.Website = url.Uri.ToString();
                        }

                        break;
                    case SyndicationElementType.Content:
                        var tempContent = await reader.ReadContent().ConfigureAwait(false);
                        switch (tempContent.Name)
                        {
                            case RssElementNames.Title:
                                feed.Name = tempContent.Value;
                                break;
                            case RssElementNames.Description:
                            case AtomElementNames.Subtitle:
                                feed.Description = tempContent.Value;
                                break;
                            case RssElementNames.Guid:
                            case AtomElementNames.Id:
                                feed.Id = tempContent.Value;
                                break;
                            default:
                                break;
                        }

                        break;
                    default:
                        break;
                }
            }

            feed.Url = feedUrl;

            if (string.IsNullOrEmpty(feed.Id))
            {
                feed.Id = feedUrl;
            }

            return new RssFeedDetail
            {
                Feed = feed,
                Articles = items,
            };
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static string GetFeedType(string feedContent, out XmlReader xmlReader)
    {
        xmlReader = XmlReader.Create(new StringReader(feedContent), new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Parse });
        var isRss = xmlReader.IsStartElement(RssElementNames.Rss, RssConstants.Rss20Namespace) &&
                        xmlReader.GetAttribute(RssElementNames.Version)!.Equals(RssConstants.Version, StringComparison.OrdinalIgnoreCase);
        if (isRss)
        {
            return "rss";
        }

        var isAtom = xmlReader.IsStartElement(AtomElementNames.Feed, AtomConstants.Atom10Namespace);
        return isAtom ? "atom" : "unknown";
    }

    internal static RssArticleBase ConvertSyndicationItemToArticle(ISyndicationItem item)
    {
        var article = new RssArticleBase
        {
            Title = string.IsNullOrEmpty(item.Title) ? "--" : item.Title.Trim().DecodeHtml(),
            Url = item.Links.FirstOrDefault(p => string.IsNullOrEmpty(p.MediaType))?.Uri.ToString() ?? string.Empty,
            Author = string.Join(", ", item.Contributors.Select(p => p.Name)),
        };
        var content = string.IsNullOrEmpty(item.EncodedContent) ? item.Description : item.EncodedContent;
        var summary = string.IsNullOrEmpty(item.Description) ? item.EncodedContent : item.Description;
        if (item is AtomEntry atomEntry && !string.IsNullOrEmpty(atomEntry.Summary))
        {
            summary = atomEntry.Summary;
            if (string.IsNullOrEmpty(content))
            {
                content = summary;
            }
        }

        content = content.FixHtml().SanitizeString();
        summary = summary.DecodeHtml().Trim().Truncate(300).ClearReturnSanitizeString();
        article.Summary = summary;
        article.Content = content;
        article.Id = string.IsNullOrEmpty(item.Id) ? article.Url : item.Id;
        article.PublishDate = item.Published.ToLocalTime().ToString();
        article.Cover = string.IsNullOrEmpty(item.Image)
            ? item.Links.FirstOrDefault(p => p.MediaType?.StartsWith("image", StringComparison.OrdinalIgnoreCase) == true)?.Uri.ToString() ?? string.Empty
            : item.Image ?? string.Empty;

        if (string.IsNullOrEmpty(article.Cover))
        {
            var cover = Toolkits.Feed.Utils.XmlUtils.GetCover(article.Content);
            article.Cover = cover ?? string.Empty;
        }

        article.SetTags(item.Categories.Select(p => p.Label));
        return article;
    }

    internal static async Task<string> GenerateOpmlContentAsync(IRssConnector service)
    {
        var (groups, feeds) = await service.GetFeedListAsync().ConfigureAwait(false);
        var buf = new StringBuilder();
        _ = buf.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        _ = buf.AppendLine("<opml version=\"2.0\">");
        _ = buf.AppendLine("  <head>");
        _ = buf.AppendLine("    <title>Reader Copilot</title>");
        _ = buf.AppendLine($"    <dateCreated>{DateTimeOffset.Now.ToString()}</dateCreated>");
        _ = buf.AppendLine("  </head>");
        _ = buf.AppendLine("  <body>");
        foreach (var group in groups)
        {
            _ = buf.AppendLine("    <outline text=\"" + group.Name + "\">");
            foreach (var feed in feeds.Where(p => p.GetGroupIds().Contains(group.Id)))
            {
                _ = buf.AppendLine($"      <outline text=\"{feed.Name}\" type=\"rss\" xmlUrl=\"{feed.Url}\" htmlUrl=\"{feed.Website}\" description=\"{feed.Description}\"/>");
            }

            _ = buf.AppendLine("    </outline>");
        }

        foreach (var feed in feeds.Where(p => p.GetGroupIds().Count == 0))
        {
            _ = buf.AppendLine($"    <outline text=\"{feed.Name}\" type=\"rss\" xmlUrl=\"{feed.Url}\" htmlUrl=\"{feed.Website}\" description=\"{feed.Description}\"/>");
        }

        _ = buf.AppendLine("  </body>");
        _ = buf.AppendLine("</opml>");

        return buf.ToString();
    }

    internal static async Task<RssClientConfiguration> GetLibraryConfigurationAsync(IKernelSettingToolkit settingToolkit)
    {
        var libFolder = settingToolkit.ReadSetting(ReaderKernel.Models.KernelSettingNames.LibraryPath, string.Empty);
        var filePath = Path.Combine(libFolder!, "__rss_config.json");
        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            if (string.IsNullOrEmpty(json))
            {
                return new RssClientConfiguration();
            }

            return JsonSerializer.Deserialize(json, JsonGenContext.Default.RssClientConfiguration)!;
        }

        return new RssClientConfiguration();
    }

    internal static async Task WriteLibraryConfigurationAsync(RssClientConfiguration config, IKernelSettingToolkit settingToolkit)
    {
        var libFolder = settingToolkit.ReadSetting(ReaderKernel.Models.KernelSettingNames.LibraryPath, string.Empty);
        var filePath = Path.Combine(libFolder!, "__rss_config.json");
        var json = JsonSerializer.Serialize(config, JsonGenContext.Default.RssClientConfiguration);
        await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
    }

    private static string RemoveBom(string content)
    {
        if (content.Contains("<?", StringComparison.OrdinalIgnoreCase))
        {
            var index = content.IndexOf("<?", StringComparison.OrdinalIgnoreCase);
            content = content.Remove(0, index);
        }

        return content;
    }
}

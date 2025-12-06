// Copyright (c) Richasy. All rights reserved.

using System.Xml;

namespace Richasy.RodelReader.Sources.Podcast.Apple.Internal;

/// <summary>
/// 播客 Feed 解析器实现.
/// </summary>
internal sealed class PodcastFeedParser : IPodcastFeedParser
{
    private const string ItunesNamespace = "http://www.itunes.com/dtds/podcast-1.0.dtd";
    private const string ContentNamespace = "http://purl.org/rss/1.0/modules/content/";

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastFeedParser"/> class.
    /// </summary>
    /// <param name="logger">日志器.</param>
    public PodcastFeedParser(ILogger logger)
    {
        _logger = Guard.NotNull(logger);
    }

    /// <inheritdoc/>
    public Task<PodcastDetail?> ParseAsync(string feedContent, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(feedContent);

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(feedContent);
            return Task.FromResult(ParseDocument(doc));
        }
        catch (XmlException ex)
        {
            _logger.LogError(ex, "Failed to parse feed XML");
            throw new ApplePodcastException($"Failed to parse feed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<PodcastDetail?> ParseAsync(Stream feedStream, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(feedStream);

        try
        {
            var doc = new XmlDocument();
            using var reader = new StreamReader(feedStream);
            var content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            doc.LoadXml(content);
            return ParseDocument(doc);
        }
        catch (XmlException ex)
        {
            _logger.LogError(ex, "Failed to parse feed XML from stream");
            throw new ApplePodcastException($"Failed to parse feed: {ex.Message}", ex);
        }
    }

    private PodcastDetail? ParseDocument(XmlDocument doc)
    {
        var nsMgr = new XmlNamespaceManager(doc.NameTable);
        nsMgr.AddNamespace("itunes", ItunesNamespace);
        nsMgr.AddNamespace("content", ContentNamespace);

        var channel = doc.SelectSingleNode("//channel");
        if (channel == null)
        {
            _logger.LogWarning("No channel element found in feed");
            return null;
        }

        var title = GetNodeText(channel, "title");
        if (string.IsNullOrEmpty(title))
        {
            _logger.LogWarning("No title found in feed");
            return null;
        }

        var description = GetNodeText(channel, "description")
            ?? GetNodeText(channel, "itunes:summary", nsMgr);

        var cover = channel.SelectSingleNode("itunes:image/@href", nsMgr)?.Value
            ?? GetNodeText(channel, "image/url");

        var author = GetNodeText(channel, "itunes:author", nsMgr)
            ?? GetNodeText(channel, "managingEditor");

        var website = GetNodeText(channel, "link");

        var categoryIds = new List<string>();
        var categoryNodes = channel.SelectNodes("itunes:category/@text", nsMgr);
        if (categoryNodes != null)
        {
            foreach (XmlNode node in categoryNodes)
            {
                if (!string.IsNullOrEmpty(node.Value))
                {
                    categoryIds.Add(node.Value);
                }
            }
        }

        // 解析单集
        var episodes = new List<PodcastEpisode>();
        var itemNodes = channel.SelectNodes("item");
        if (itemNodes != null)
        {
            foreach (XmlNode item in itemNodes)
            {
                var episode = ParseEpisode(item, nsMgr);
                if (episode != null)
                {
                    episodes.Add(episode);
                }
            }
        }

        _logger.LogDebug("Parsed feed '{Title}' with {EpisodeCount} episodes", title, episodes.Count);

        return new PodcastDetail
        {
            Id = GetNodeText(channel, "itunes:new-feed-url", nsMgr) ?? website ?? title,
            Name = title,
            Description = description,
            Cover = cover,
            Author = author,
            Website = website,
            CategoryIds = categoryIds,
            Episodes = episodes,
        };
    }

    private static PodcastEpisode? ParseEpisode(XmlNode item, XmlNamespaceManager nsMgr)
    {
        var title = GetNodeText(item, "title");
        if (string.IsNullOrEmpty(title))
        {
            return null;
        }

        var guid = GetNodeText(item, "guid") ?? title;

        var description = GetNodeText(item, "content:encoded", nsMgr)
            ?? GetNodeText(item, "description")
            ?? GetNodeText(item, "itunes:summary", nsMgr);

        // 解析音频 URL
        var enclosureNode = item.SelectSingleNode("enclosure");
        var audioUrl = enclosureNode?.Attributes?["url"]?.Value;
        var audioMimeType = enclosureNode?.Attributes?["type"]?.Value;
        var fileSizeStr = enclosureNode?.Attributes?["length"]?.Value;
        long? fileSize = long.TryParse(fileSizeStr, out var fs) ? fs : null;

        // 解析时长
        var durationStr = GetNodeText(item, "itunes:duration", nsMgr);
        int? duration = ParseDuration(durationStr);

        // 解析发布日期
        var pubDateStr = GetNodeText(item, "pubDate");
        DateTimeOffset? pubDate = DateTimeOffset.TryParse(pubDateStr, out var pd) ? pd : null;

        // 解析季数和集数
        var seasonStr = GetNodeText(item, "itunes:season", nsMgr);
        var episodeStr = GetNodeText(item, "itunes:episode", nsMgr);
        int? season = int.TryParse(seasonStr, out var s) ? s : null;
        int? episode = int.TryParse(episodeStr, out var e) ? e : null;

        // 单集封面
        var episodeCover = item.SelectSingleNode("itunes:image/@href", nsMgr)?.Value;

        // 单集类型
        var episodeType = GetNodeText(item, "itunes:episodeType", nsMgr);

        // 是否显式内容
        var explicitStr = GetNodeText(item, "itunes:explicit", nsMgr);
        var isExplicit = explicitStr?.Equals("yes", StringComparison.OrdinalIgnoreCase) == true
            || explicitStr?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

        return new PodcastEpisode
        {
            Id = guid,
            Title = title,
            Description = description,
            AudioUrl = audioUrl,
            AudioMimeType = audioMimeType,
            FileSizeInBytes = fileSize,
            DurationInSeconds = duration,
            PublishedDate = pubDate,
            Season = season,
            Episode = episode,
            Cover = episodeCover,
            EpisodeType = episodeType,
            IsExplicit = isExplicit,
        };
    }

    private static string? GetNodeText(XmlNode parent, string xpath, XmlNamespaceManager? nsMgr = null)
    {
        var node = nsMgr != null
            ? parent.SelectSingleNode(xpath, nsMgr)
            : parent.SelectSingleNode(xpath);
        return node?.InnerText;
    }

    private static int? ParseDuration(string? durationStr)
    {
        if (string.IsNullOrEmpty(durationStr))
        {
            return null;
        }

        // 可能是秒数或 HH:MM:SS 格式
        if (int.TryParse(durationStr, out var seconds))
        {
            return seconds;
        }

        var parts = durationStr.Split(':');
        return parts.Length switch
        {
            2 when int.TryParse(parts[0], out var m) && int.TryParse(parts[1], out var s) => (m * 60) + s,
            3 when int.TryParse(parts[0], out var h) && int.TryParse(parts[1], out var m) && int.TryParse(parts[2], out var s) => (h * 3600) + (m * 60) + s,
            _ => null,
        };
    }
}

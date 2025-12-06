// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Podcast.Apple.Internal;

/// <summary>
/// 播客搜索器实现.
/// </summary>
internal sealed class PodcastSearcher : IPodcastSearcher
{
    private readonly IPodcastDispatcher _dispatcher;
    private readonly ApplePodcastClientOptions _options;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastSearcher"/> class.
    /// </summary>
    /// <param name="dispatcher">HTTP 分发器.</param>
    /// <param name="options">客户端配置.</param>
    /// <param name="logger">日志器.</param>
    public PodcastSearcher(
        IPodcastDispatcher dispatcher,
        ApplePodcastClientOptions options,
        ILogger logger)
    {
        _dispatcher = Guard.NotNull(dispatcher);
        _options = Guard.NotNull(options);
        _logger = Guard.NotNull(logger);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PodcastSummary>> SearchAsync(
        string keyword,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(keyword);

        var actualLimit = limit ?? _options.DefaultLimit;
        _logger.LogDebug("Searching podcasts with keyword '{Keyword}' and limit {Limit}", keyword, actualLimit);

        var uri = UriHelper.BuildSearchUri(keyword, actualLimit);
        var response = await _dispatcher.GetJsonAsync(uri, ApplePodcastJsonContext.Default.ITunesSearchResponse, cancellationToken).ConfigureAwait(false);

        if (response?.results == null)
        {
            _logger.LogWarning("Empty search response");
            return [];
        }

        var result = new List<PodcastSummary>();
        foreach (var item in response.results)
        {
            var id = item.trackId.ToString();
            var name = item.trackName;

            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            result.Add(new PodcastSummary
            {
                Id = id,
                Name = name,
                Cover = item.artworkUrl600 ?? item.artworkUrl100 ?? item.artworkUrl60,
                Artist = item.artistName,
                FeedUrl = item.feedUrl,
                ITunesUrl = item.trackViewUrl,
            });
        }

        _logger.LogInformation("Found {Count} podcasts for keyword '{Keyword}'", result.Count, keyword);
        return result;
    }
}

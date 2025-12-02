// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ApplePodcast.Internal;

/// <summary>
/// 播客详情提供器实现.
/// </summary>
internal sealed class PodcastDetailProvider : IPodcastDetailProvider
{
    private readonly IPodcastDispatcher _dispatcher;
    private readonly IPodcastFeedParser _parser;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastDetailProvider"/> class.
    /// </summary>
    /// <param name="dispatcher">HTTP 分发器.</param>
    /// <param name="parser">Feed 解析器.</param>
    /// <param name="logger">日志器.</param>
    public PodcastDetailProvider(
        IPodcastDispatcher dispatcher,
        IPodcastFeedParser parser,
        ILogger logger)
    {
        _dispatcher = Guard.NotNull(dispatcher);
        _parser = Guard.NotNull(parser);
        _logger = Guard.NotNull(logger);
    }

    /// <inheritdoc/>
    public async Task<PodcastDetail?> GetDetailByIdAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrEmpty(podcastId);

        _logger.LogDebug("Getting podcast detail by ID {PodcastId}", podcastId);

        // 首先通过 lookup API 获取 Feed URL
        var lookupUri = UriHelper.BuildLookupUri(podcastId);
        var response = await _dispatcher.GetJsonAsync(lookupUri, ApplePodcastJsonContext.Default.ITunesSearchResponse, cancellationToken).ConfigureAwait(false);

        var result = response?.results?.FirstOrDefault();
        if (result == null)
        {
            _logger.LogWarning("No result found for podcast ID {PodcastId}", podcastId);
            return null;
        }

        var feedUrl = result.feedUrl;
        if (string.IsNullOrEmpty(feedUrl))
        {
            _logger.LogWarning("No feed URL found for podcast ID {PodcastId}", podcastId);
            return null;
        }

        // 解析 Feed 获取详情
        var detail = await GetDetailByFeedUrlAsync(feedUrl, cancellationToken).ConfigureAwait(false);

        if (detail != null)
        {
            // 用 iTunes 数据补充信息
            return detail with
            {
                Id = podcastId,
                FeedUrl = feedUrl,
                Cover = detail.Cover ?? result.artworkUrl600 ?? result.artworkUrl100,
            };
        }

        return detail;
    }

    /// <inheritdoc/>
    public async Task<PodcastDetail?> GetDetailByFeedUrlAsync(string feedUrl, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrEmpty(feedUrl);

        _logger.LogDebug("Getting podcast detail from feed URL {FeedUrl}", feedUrl);

        try
        {
            var feedContent = await _dispatcher.GetStringAsync(new Uri(feedUrl), cancellationToken).ConfigureAwait(false);
            var detail = await _parser.ParseAsync(feedContent, cancellationToken).ConfigureAwait(false);

            if (detail != null)
            {
                return detail with { FeedUrl = feedUrl };
            }

            return null;
        }
        catch (ApplePodcastException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get podcast detail from feed URL {FeedUrl}", feedUrl);
            throw new ApplePodcastException($"Failed to get podcast detail: {ex.Message}", ex);
        }
    }
}

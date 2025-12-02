// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ApplePodcast.Internal;

/// <summary>
/// 分类提供器实现.
/// </summary>
internal sealed class CategoryProvider : ICategoryProvider
{
    private readonly IPodcastDispatcher _dispatcher;
    private readonly ApplePodcastClientOptions _options;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryProvider"/> class.
    /// </summary>
    /// <param name="dispatcher">HTTP 分发器.</param>
    /// <param name="options">客户端配置.</param>
    /// <param name="logger">日志器.</param>
    public CategoryProvider(
        IPodcastDispatcher dispatcher,
        ApplePodcastClientOptions options,
        ILogger logger)
    {
        _dispatcher = Guard.NotNull(dispatcher);
        _options = Guard.NotNull(options);
        _logger = Guard.NotNull(logger);
    }

    /// <inheritdoc/>
    public IReadOnlyList<PodcastCategory> GetCategories()
    {
        _logger.LogDebug("Getting all podcast categories");
        return PodcastCategory.GetAllCategories();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PodcastSummary>> GetTopPodcastsAsync(
        string categoryId,
        string? region = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrEmpty(categoryId);

        var actualRegion = region ?? _options.DefaultRegion;
        var actualLimit = limit ?? _options.DefaultLimit;

        _logger.LogDebug(
            "Getting top podcasts for category {CategoryId} in region {Region} with limit {Limit}",
            categoryId,
            actualRegion,
            actualLimit);

        var uri = UriHelper.BuildTopPodcastsUri(actualRegion, actualLimit, categoryId);
        var response = await _dispatcher.GetJsonAsync(uri, ApplePodcastJsonContext.Default.ITunesCategoryResponse, cancellationToken).ConfigureAwait(false);

        if (response?.feed?.entry == null)
        {
            _logger.LogWarning("Empty response for top podcasts");
            return [];
        }

        var result = new List<PodcastSummary>();
        foreach (var entry in response.feed.entry)
        {
            var id = entry.Id?.Attributes?.ImId;
            var name = entry.ImName?.Label;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
            {
                continue;
            }

            result.Add(new PodcastSummary
            {
                Id = id,
                Name = name,
                Cover = entry.ImImage?.LastOrDefault()?.Label,
                Description = entry.Summary?.Label,
                Artist = entry.ImArtist?.Label,
                ITunesUrl = entry.Id?.Label,
            });
        }

        _logger.LogInformation("Retrieved {Count} top podcasts", result.Count);
        return result;
    }
}

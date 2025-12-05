// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Loader;

/// <summary>
/// 插件注册表，用于管理和查询已加载的插件功能.
/// </summary>
public sealed class PluginRegistry
{
    private readonly IPluginLoader _loader;
    private readonly ILogger<PluginRegistry> _logger;

    /// <summary>
    /// 初始化 <see cref="PluginRegistry"/> 类的新实例.
    /// </summary>
    public PluginRegistry(IPluginLoader loader, ILogger<PluginRegistry> logger)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 获取所有书籍刮削器功能.
    /// </summary>
    /// <returns>刮削器功能列表.</returns>
    public IReadOnlyList<IBookScraperFeature> GetBookScrapers()
    {
        var scrapers = new List<IBookScraperFeature>();
        var plugins = _loader.GetPluginsByCapability(PluginCapability.BookScraper);

        foreach (var loadedPlugin in plugins)
        {
            var allFeatures = loadedPlugin.Plugin.GetAllFeatures();
            foreach (var feature in allFeatures)
            {
                if (feature is IBookScraperFeature scraper)
                {
                    scrapers.Add(scraper);
                    _logger.LogDebug(
                        "注册刮削器: {ScraperId} ({ScraperName}) from {PluginId}",
                        scraper.FeatureId,
                        scraper.FeatureName,
                        loadedPlugin.Plugin.Metadata.Id);
                }
            }
        }

        _logger.LogInformation("共找到 {Count} 个书籍刮削器", scrapers.Count);
        return scrapers.AsReadOnly();
    }

    /// <summary>
    /// 获取指定 ID 的刮削器.
    /// </summary>
    /// <param name="scraperId">刮削器 ID.</param>
    /// <returns>刮削器功能，如果未找到则返回 null.</returns>
    public IBookScraperFeature? GetBookScraper(string scraperId)
    {
        var scrapers = GetBookScrapers();
        return scrapers.FirstOrDefault(s =>
            s.FeatureId.Equals(scraperId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取指定类型的所有功能特性.
    /// </summary>
    /// <typeparam name="TFeature">功能特性类型.</typeparam>
    /// <returns>功能特性列表.</returns>
    public IReadOnlyList<TFeature> GetFeatures<TFeature>()
        where TFeature : class, IPluginFeature
    {
        var features = new List<TFeature>();
        var allPlugins = _loader.GetAllLoadedPlugins();

        foreach (var loadedPlugin in allPlugins)
        {
            var feature = loadedPlugin.Plugin.GetFeature<TFeature>();
            if (feature is not null)
            {
                features.Add(feature);
            }
        }

        return features.AsReadOnly();
    }

    /// <summary>
    /// 获取插件提供的所有功能特性.
    /// </summary>
    /// <param name="pluginId">插件 ID.</param>
    /// <returns>功能特性列表.</returns>
    public IReadOnlyList<IPluginFeature> GetPluginFeatures(string pluginId)
    {
        var plugin = _loader.GetLoadedPlugin(pluginId);
        if (plugin is null)
        {
            _logger.LogWarning("未找到插件: {PluginId}", pluginId);
            return Array.Empty<IPluginFeature>();
        }

        return plugin.Plugin.GetAllFeatures();
    }
}

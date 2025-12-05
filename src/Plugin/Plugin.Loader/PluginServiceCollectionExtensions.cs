// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Loader;

/// <summary>
/// 插件系统的依赖注入扩展.
/// </summary>
public static class PluginServiceCollectionExtensions
{
    /// <summary>
    /// 添加插件系统服务.
    /// </summary>
    /// <param name="services">服务集合.</param>
    /// <param name="configure">配置委托.</param>
    /// <returns>服务集合.</returns>
    public static IServiceCollection AddPluginSystem(
        this IServiceCollection services,
        Action<PluginLoaderOptions>? configure = null)
    {
        var options = new PluginLoaderOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IPluginLoader, PluginLoader>();
        services.AddSingleton<PluginRegistry>();

        return services;
    }

    /// <summary>
    /// 添加插件系统服务并立即加载插件.
    /// </summary>
    /// <param name="services">服务集合.</param>
    /// <param name="configure">配置委托.</param>
    /// <returns>服务集合.</returns>
    public static IServiceCollection AddPluginSystemWithAutoLoad(
        this IServiceCollection services,
        Action<PluginLoaderOptions>? configure = null)
    {
        services.AddPluginSystem(configure);

        // 注册后台服务用于自动加载插件
        services.AddHostedService<PluginAutoLoadService>();

        return services;
    }
}

/// <summary>
/// 插件自动加载后台服务.
/// </summary>
internal sealed class PluginAutoLoadService : Microsoft.Extensions.Hosting.IHostedService
{
    private readonly IPluginLoader _loader;
    private readonly PluginLoaderOptions _options;
    private readonly ILogger<PluginAutoLoadService> _logger;

    public PluginAutoLoadService(
        IPluginLoader loader,
        PluginLoaderOptions options,
        ILogger<PluginAutoLoadService> logger)
    {
        _loader = loader;
        _options = options;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("开始自动加载插件...");

        if (!Directory.Exists(_options.PluginDirectory))
        {
            _logger.LogWarning("插件目录不存在，跳过自动加载: {Directory}", _options.PluginDirectory);
            return;
        }

        var results = await _loader.LoadPluginsFromDirectoryAsync(
            _options.PluginDirectory,
            _options.SearchPattern,
            cancellationToken).ConfigureAwait(false);

        var successCount = results.Count(r => r.Success);
        _logger.LogInformation("自动加载完成 - 成功: {Success}, 失败: {Failed}",
            successCount,
            results.Count - successCount);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("插件自动加载服务已停止");
        return Task.CompletedTask;
    }
}

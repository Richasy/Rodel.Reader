// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Loader;

/// <summary>
/// 默认插件加载器实现.
/// </summary>
public sealed class PluginLoader : IPluginLoader, IDisposable
{
    private readonly Dictionary<string, LoadedPlugin> _loadedPlugins = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _serviceProvider;
    private readonly PluginLoaderOptions _options;
    private readonly ILogger<PluginLoader> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// 初始化 <see cref="PluginLoader"/> 类的新实例.
    /// </summary>
    public PluginLoader(
        IServiceProvider serviceProvider,
        PluginLoaderOptions options,
        ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<PluginLoader>();

        _logger.LogInformation(
            "插件加载器已初始化 - 插件目录: {PluginDir}, 数据目录: {DataDir}, 宿主版本: {HostVersion}",
            _options.PluginDirectory,
            _options.PluginDataDirectory,
            _options.HostVersion);
    }

    /// <inheritdoc/>
    public async Task<PluginLoadResult> LoadPluginAsync(string assemblyPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);

        var fullPath = Path.GetFullPath(assemblyPath);
        _logger.LogDebug("开始加载插件: {AssemblyPath}", fullPath);

        if (!File.Exists(fullPath))
        {
            var error = $"插件文件不存在: {fullPath}";
            _logger.LogError("{Error}", error);
            return PluginLoadResult.Failed(error);
        }

        try
        {
            // 创建隔离的加载上下文
            var loadContext = new PluginLoadContext(fullPath, _logger);

            // 加载程序集
            var assembly = loadContext.LoadFromAssemblyPath(fullPath);
            _logger.LogDebug("程序集已加载: {AssemblyName}", assembly.FullName);

            // 查找 IPlugin 实现
            var pluginType = FindPluginType(assembly);
            if (pluginType is null)
            {
                loadContext.Unload();
                var error = $"程序集中未找到 IPlugin 实现: {fullPath}";
                _logger.LogWarning("{Error}", error);
                return PluginLoadResult.Failed(error);
            }

            // 创建插件实例
            var plugin = CreatePluginInstance(pluginType);
            if (plugin is null)
            {
                loadContext.Unload();
                var error = $"无法创建插件实例: {pluginType.FullName}";
                _logger.LogError("{Error}", error);
                return PluginLoadResult.Failed(error);
            }

            _logger.LogInformation(
                "发现插件: {PluginId} - {PluginName} v{Version} by {Author}",
                plugin.Metadata.Id,
                plugin.Metadata.Name,
                plugin.Metadata.Version,
                plugin.Metadata.Author ?? "Unknown");

            _logger.LogDebug("插件能力: {Capabilities}", plugin.Capabilities);

            // 检查版本兼容性
            if (!CheckVersionCompatibility(plugin))
            {
                loadContext.Unload();
                plugin.Dispose();
                var error = $"插件 {plugin.Metadata.Id} 要求最低宿主版本 {plugin.Metadata.MinHostVersion}，当前版本 {_options.HostVersion}";
                _logger.LogError("{Error}", error);
                return PluginLoadResult.Failed(error);
            }

            // 检查是否已加载
            lock (_lock)
            {
                if (_loadedPlugins.ContainsKey(plugin.Metadata.Id))
                {
                    loadContext.Unload();
                    plugin.Dispose();
                    var error = $"插件已加载: {plugin.Metadata.Id}";
                    _logger.LogWarning("{Error}", error);
                    return PluginLoadResult.Failed(error);
                }
            }

            // 初始化插件
            var initContext = CreateInitializationContext(plugin, fullPath);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.LoadTimeoutSeconds));

            try
            {
                await plugin.InitializeAsync(initContext).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                loadContext.Unload();
                plugin.Dispose();
                var error = $"插件初始化超时: {plugin.Metadata.Id}";
                _logger.LogError("{Error}", error);
                return PluginLoadResult.Failed(error);
            }

            // 注册插件
            var loadedPlugin = new LoadedPlugin(plugin, fullPath, loadContext);
            lock (_lock)
            {
                _loadedPlugins[plugin.Metadata.Id] = loadedPlugin;
            }

            _logger.LogInformation(
                "插件加载成功: {PluginId} - 提供 {FeatureCount} 个功能特性",
                plugin.Metadata.Id,
                plugin.GetAllFeatures().Count);

            return PluginLoadResult.Succeeded(plugin);
        }
        catch (Exception ex)
        {
            var error = $"加载插件时发生错误: {fullPath}";
            _logger.LogError(ex, "{Error}", error);
            return PluginLoadResult.Failed(error, ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PluginLoadResult>> LoadPluginsFromDirectoryAsync(
        string directory,
        string searchPattern = "*.dll",
        CancellationToken cancellationToken = default)
    {
        var results = new List<PluginLoadResult>();
        var fullPath = Path.GetFullPath(directory);

        _logger.LogInformation("开始从目录扫描插件: {Directory}", fullPath);

        if (!Directory.Exists(fullPath))
        {
            _logger.LogWarning("插件目录不存在: {Directory}", fullPath);
            return results.AsReadOnly();
        }

        var searchOption = _options.SearchSubdirectories
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        var dllFiles = Directory.GetFiles(fullPath, searchPattern, searchOption);
        _logger.LogDebug("找到 {Count} 个 DLL 文件", dllFiles.Length);

        foreach (var dllFile in dllFiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("插件加载已取消");
                break;
            }

            // 跳过明显不是插件的系统 DLL
            var fileName = Path.GetFileName(dllFile);
            if (ShouldSkipFile(fileName))
            {
                _logger.LogTrace("跳过系统 DLL: {FileName}", fileName);
                continue;
            }

            var result = await LoadPluginAsync(dllFile, cancellationToken).ConfigureAwait(false);
            results.Add(result);

            if (!result.Success && !_options.ContinueOnLoadError)
            {
                _logger.LogWarning("插件加载失败且配置为不继续，停止加载");
                break;
            }
        }

        var successCount = results.Count(r => r.Success);
        _logger.LogInformation(
            "插件扫描完成 - 成功: {Success}/{Total}",
            successCount,
            results.Count);

        return results.AsReadOnly();
    }

    /// <inheritdoc/>
    public bool UnloadPlugin(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        _logger.LogDebug("尝试卸载插件: {PluginId}", pluginId);

        LoadedPlugin? loadedPlugin;
        lock (_lock)
        {
            if (!_loadedPlugins.TryGetValue(pluginId, out loadedPlugin))
            {
                _logger.LogWarning("未找到要卸载的插件: {PluginId}", pluginId);
                return false;
            }

            _loadedPlugins.Remove(pluginId);
        }

        try
        {
            loadedPlugin.Dispose();
            _logger.LogInformation("插件已卸载: {PluginId}", pluginId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "卸载插件时发生错误: {PluginId}", pluginId);
            return false;
        }
    }

    /// <inheritdoc/>
    public LoadedPlugin? GetLoadedPlugin(string pluginId)
    {
        lock (_lock)
        {
            return _loadedPlugins.TryGetValue(pluginId, out var plugin) ? plugin : null;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<LoadedPlugin> GetAllLoadedPlugins()
    {
        lock (_lock)
        {
            return _loadedPlugins.Values.ToList().AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<LoadedPlugin> GetPluginsByCapability(PluginCapability capability)
    {
        lock (_lock)
        {
            return _loadedPlugins.Values
                .Where(p => p.Plugin.Capabilities.HasFlag(capability))
                .ToList()
                .AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _logger.LogInformation("正在释放插件加载器...");

        lock (_lock)
        {
            foreach (var (id, plugin) in _loadedPlugins)
            {
                try
                {
                    plugin.Dispose();
                    _logger.LogDebug("已释放插件: {PluginId}", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "释放插件时发生错误: {PluginId}", id);
                }
            }

            _loadedPlugins.Clear();
        }

        _logger.LogInformation("插件加载器已释放");
        _disposed = true;
    }

    private static Type? FindPluginType(Assembly assembly)
    {
        try
        {
            var pluginInterface = typeof(IPlugin);
            return assembly.GetTypes()
                .FirstOrDefault(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    pluginInterface.IsAssignableFrom(t));
        }
        catch (ReflectionTypeLoadException ex)
        {
            // 某些类型可能无法加载，检查已加载的类型
            var pluginInterface = typeof(IPlugin);
            return ex.Types
                .Where(t => t is not null)
                .FirstOrDefault(t =>
                    t!.IsClass &&
                    !t.IsAbstract &&
                    pluginInterface.IsAssignableFrom(t));
        }
    }

    private static IPlugin? CreatePluginInstance(Type pluginType)
    {
        try
        {
            return Activator.CreateInstance(pluginType) as IPlugin;
        }
        catch
        {
            return null;
        }
    }

    private static bool ShouldSkipFile(string fileName)
    {
        // 跳过常见的系统和框架 DLL
        var skipPrefixes = new[]
        {
            "System.",
            "Microsoft.",
            "netstandard",
            "mscorlib",
            "WindowsBase",
            "PresentationCore",
            "PresentationFramework",
        };

        return skipPrefixes.Any(prefix =>
            fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private bool CheckVersionCompatibility(IPlugin plugin)
    {
        if (plugin.Metadata.MinHostVersion is null)
        {
            return true;
        }

        return _options.HostVersion >= plugin.Metadata.MinHostVersion;
    }

    private PluginInitializationContext CreateInitializationContext(IPlugin plugin, string assemblyPath)
    {
        var pluginDir = Path.GetDirectoryName(assemblyPath) ?? string.Empty;
        var dataDir = Path.Combine(_options.PluginDataDirectory, plugin.Metadata.Id);

        // 确保数据目录存在
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
            _logger.LogDebug("创建插件数据目录: {DataDir}", dataDir);
        }

        // 为插件创建专用的 Logger
        var pluginLogger = _loggerFactory.CreateLogger($"Plugin.{plugin.Metadata.Id}");

        return new PluginInitializationContext(
            _serviceProvider,
            pluginLogger,
            pluginDir,
            dataDir,
            _options.HostVersion);
    }
}

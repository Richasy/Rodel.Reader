// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Abstractions;

/// <summary>
/// 插件基类，提供常用功能的默认实现.
/// 插件开发者可继承此类以简化开发.
/// </summary>
public abstract class PluginBase : IPlugin
{
    private readonly List<IPluginFeature> _features = [];
    private bool _disposed;

    /// <summary>
    /// 获取日志记录器.
    /// </summary>
    protected ILogger? Logger { get; private set; }

    /// <summary>
    /// 获取插件初始化上下文.
    /// </summary>
    protected PluginInitializationContext? Context { get; private set; }

    /// <inheritdoc/>
    public abstract PluginMetadata Metadata { get; }

    /// <inheritdoc/>
    public abstract PluginCapability Capabilities { get; }

    /// <inheritdoc/>
    public bool IsInitialized { get; private set; }

    /// <inheritdoc/>
    public async Task InitializeAsync(PluginInitializationContext context)
    {
        if (IsInitialized)
        {
            return;
        }

        Context = context ?? throw new ArgumentNullException(nameof(context));
        Logger = context.Logger;

        Logger?.LogDebug("正在初始化插件: {PluginId} v{Version}", Metadata.Id, Metadata.Version);

        try
        {
            await OnInitializeAsync(context).ConfigureAwait(false);
            IsInitialized = true;
            Logger?.LogInformation("插件初始化成功: {PluginId}", Metadata.Id);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "插件初始化失败: {PluginId}", Metadata.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public TFeature? GetFeature<TFeature>()
        where TFeature : class, IPluginFeature
    {
        return _features.OfType<TFeature>().FirstOrDefault();
    }

    /// <inheritdoc/>
    public IReadOnlyList<IPluginFeature> GetAllFeatures()
        => _features.AsReadOnly();

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 注册功能特性.
    /// 应在 <see cref="OnInitializeAsync"/> 中调用.
    /// </summary>
    /// <param name="feature">要注册的功能特性.</param>
    protected void RegisterFeature(IPluginFeature feature)
    {
        ArgumentNullException.ThrowIfNull(feature);
        _features.Add(feature);
        Logger?.LogDebug("注册功能特性: {FeatureId} ({FeatureType})", feature.FeatureId, feature.GetType().Name);
    }

    /// <summary>
    /// 派生类实现初始化逻辑.
    /// </summary>
    /// <param name="context">初始化上下文.</param>
    /// <returns>初始化任务.</returns>
    protected abstract Task OnInitializeAsync(PluginInitializationContext context);

    /// <summary>
    /// 释放资源.
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (var feature in _features)
            {
                if (feature is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _features.Clear();
            Logger?.LogDebug("插件已释放: {PluginId}", Metadata.Id);
        }

        _disposed = true;
    }
}

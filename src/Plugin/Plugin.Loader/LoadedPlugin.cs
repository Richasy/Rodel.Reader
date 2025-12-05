// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Loader;

/// <summary>
/// 已加载的插件信息.
/// </summary>
public sealed class LoadedPlugin : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// 初始化 <see cref="LoadedPlugin"/> 类的新实例.
    /// </summary>
    internal LoadedPlugin(
        IPlugin plugin,
        string assemblyPath,
        PluginLoadContext? loadContext)
    {
        Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        AssemblyPath = assemblyPath ?? throw new ArgumentNullException(nameof(assemblyPath));
        LoadContext = loadContext;
        LoadedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 获取插件实例.
    /// </summary>
    public IPlugin Plugin { get; }

    /// <summary>
    /// 获取插件程序集路径.
    /// </summary>
    public string AssemblyPath { get; }

    /// <summary>
    /// 获取加载时间.
    /// </summary>
    public DateTimeOffset LoadedAt { get; }

    /// <summary>
    /// 获取加载上下文（用于卸载）.
    /// </summary>
    internal PluginLoadContext? LoadContext { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Plugin.Dispose();

        // 卸载程序集加载上下文
        LoadContext?.Unload();

        _disposed = true;
    }
}

// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Loader;

/// <summary>
/// 插件加载上下文，使用 <see cref="AssemblyLoadContext"/> 实现插件隔离加载.
/// </summary>
internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly ILogger? _logger;

    /// <summary>
    /// 初始化 <see cref="PluginLoadContext"/> 类的新实例.
    /// </summary>
    /// <param name="pluginPath">插件 DLL 的完整路径.</param>
    /// <param name="logger">日志记录器.</param>
    public PluginLoadContext(string pluginPath, ILogger? logger = null)
        : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
        _logger = logger;
        PluginPath = pluginPath;
    }

    /// <summary>
    /// 获取插件路径.
    /// </summary>
    public string PluginPath { get; }

    /// <inheritdoc/>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        _logger?.LogTrace("尝试加载程序集: {AssemblyName}", assemblyName.FullName);

        // 优先从插件目录解析
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath is not null)
        {
            _logger?.LogDebug("从插件目录加载程序集: {AssemblyPath}", assemblyPath);
            return LoadFromAssemblyPath(assemblyPath);
        }

        // 检查是否是共享程序集（Plugin.Abstractions 等）
        // 这些程序集应该从默认上下文加载，以确保类型兼容性
        if (IsSharedAssembly(assemblyName))
        {
            _logger?.LogDebug("使用默认上下文加载共享程序集: {AssemblyName}", assemblyName.Name);
            return null; // 返回 null 表示使用默认加载上下文
        }

        _logger?.LogTrace("程序集将由默认上下文加载: {AssemblyName}", assemblyName.Name);
        return null;
    }

    /// <inheritdoc/>
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath is not null)
        {
            _logger?.LogDebug("加载非托管 DLL: {LibraryPath}", libraryPath);
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// 检查是否是共享程序集.
    /// 共享程序集需要在宿主和插件之间保持类型一致性.
    /// </summary>
    private static bool IsSharedAssembly(AssemblyName assemblyName)
    {
        var name = assemblyName.Name;
        if (name is null)
        {
            return false;
        }

        // 核心共享程序集列表
        var sharedAssemblies = new[]
        {
            "Richasy.RodelReader.Plugin.Abstractions",
            "Microsoft.Extensions.Logging.Abstractions",
            "Microsoft.Extensions.Logging",
            "Microsoft.Extensions.DependencyInjection.Abstractions",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.Http",
            "System.Text.Json",
        };

        return sharedAssemblies.Any(shared =>
            name.Equals(shared, StringComparison.OrdinalIgnoreCase));
    }
}

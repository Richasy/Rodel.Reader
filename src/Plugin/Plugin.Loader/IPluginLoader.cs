// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Loader;

/// <summary>
/// 插件加载器接口.
/// </summary>
public interface IPluginLoader
{
    /// <summary>
    /// 从 DLL 文件加载插件.
    /// </summary>
    /// <param name="assemblyPath">DLL 文件路径.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>加载结果.</returns>
    Task<PluginLoadResult> LoadPluginAsync(string assemblyPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从目录扫描并加载所有插件.
    /// </summary>
    /// <param name="directory">插件目录.</param>
    /// <param name="searchPattern">搜索模式，默认为 "*.dll".</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>加载结果列表.</returns>
    Task<IReadOnlyList<PluginLoadResult>> LoadPluginsFromDirectoryAsync(
        string directory,
        string searchPattern = "*.dll",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 卸载插件.
    /// </summary>
    /// <param name="pluginId">插件 ID.</param>
    /// <returns>是否卸载成功.</returns>
    bool UnloadPlugin(string pluginId);

    /// <summary>
    /// 获取已加载的插件.
    /// </summary>
    /// <param name="pluginId">插件 ID.</param>
    /// <returns>插件实例，如果未找到则返回 null.</returns>
    LoadedPlugin? GetLoadedPlugin(string pluginId);

    /// <summary>
    /// 获取所有已加载的插件.
    /// </summary>
    /// <returns>插件列表.</returns>
    IReadOnlyList<LoadedPlugin> GetAllLoadedPlugins();

    /// <summary>
    /// 获取具有指定能力的所有插件.
    /// </summary>
    /// <param name="capability">能力标识.</param>
    /// <returns>符合条件的插件列表.</returns>
    IReadOnlyList<LoadedPlugin> GetPluginsByCapability(PluginCapability capability);
}

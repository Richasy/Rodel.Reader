// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Loader;

/// <summary>
/// 插件加载器配置.
/// </summary>
public sealed class PluginLoaderOptions
{
    /// <summary>
    /// 插件目录路径.
    /// </summary>
    public string PluginDirectory { get; set; } = "plugins";

    /// <summary>
    /// 插件数据存储根目录.
    /// </summary>
    public string PluginDataDirectory { get; set; } = "plugin-data";

    /// <summary>
    /// 宿主应用版本.
    /// </summary>
    public Version HostVersion { get; set; } = new Version(1, 0, 0);

    /// <summary>
    /// 是否在加载失败时继续加载其他插件.
    /// </summary>
    public bool ContinueOnLoadError { get; set; } = true;

    /// <summary>
    /// 插件 DLL 搜索模式.
    /// </summary>
    public string SearchPattern { get; set; } = "*.dll";

    /// <summary>
    /// 是否递归搜索子目录.
    /// </summary>
    public bool SearchSubdirectories { get; set; } = true;

    /// <summary>
    /// 插件加载超时时间（秒）.
    /// </summary>
    public int LoadTimeoutSeconds { get; set; } = 30;
}

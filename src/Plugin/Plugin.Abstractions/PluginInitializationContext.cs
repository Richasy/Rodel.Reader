// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Abstractions;

/// <summary>
/// 插件初始化上下文，提供插件初始化所需的服务和配置.
/// </summary>
public sealed class PluginInitializationContext
{
    /// <summary>
    /// 初始化 <see cref="PluginInitializationContext"/> 类的新实例.
    /// </summary>
    /// <param name="services">服务提供程序.</param>
    /// <param name="logger">日志记录器.</param>
    /// <param name="pluginDirectory">插件所在目录.</param>
    /// <param name="dataDirectory">插件数据存储目录.</param>
    /// <param name="hostVersion">宿主应用版本.</param>
    public PluginInitializationContext(
        IServiceProvider services,
        ILogger logger,
        string pluginDirectory,
        string dataDirectory,
        Version hostVersion)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        PluginDirectory = pluginDirectory ?? throw new ArgumentNullException(nameof(pluginDirectory));
        DataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));
        HostVersion = hostVersion ?? throw new ArgumentNullException(nameof(hostVersion));
    }

    /// <summary>
    /// 获取服务提供程序.
    /// 插件可通过此获取宿主提供的服务（如 HttpClientFactory 等）.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// 获取日志记录器.
    /// 插件应使用此记录器输出日志，以便宿主统一管理.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// 获取插件所在的目录路径.
    /// 插件可从此目录读取自身附带的资源文件.
    /// </summary>
    public string PluginDirectory { get; }

    /// <summary>
    /// 获取插件专用的数据存储目录.
    /// 插件应将配置、缓存等数据存储在此目录.
    /// </summary>
    public string DataDirectory { get; }

    /// <summary>
    /// 获取宿主应用的版本.
    /// 插件可根据此版本调整行为以保持兼容性.
    /// </summary>
    public Version HostVersion { get; }
}

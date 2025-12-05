// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Abstractions;

/// <summary>
/// 插件接口，所有插件必须实现此接口.
/// 插件加载器将通过此接口发现和管理插件.
/// </summary>
public interface IPlugin : IDisposable
{
    /// <summary>
    /// 获取插件元数据.
    /// </summary>
    PluginMetadata Metadata { get; }

    /// <summary>
    /// 获取插件支持的能力.
    /// </summary>
    PluginCapability Capabilities { get; }

    /// <summary>
    /// 获取插件是否已初始化.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// 初始化插件.
    /// 在插件加载后调用，用于执行必要的初始化操作.
    /// </summary>
    /// <param name="context">插件初始化上下文.</param>
    /// <returns>初始化任务.</returns>
    Task InitializeAsync(PluginInitializationContext context);

    /// <summary>
    /// 获取指定类型的功能特性.
    /// </summary>
    /// <typeparam name="TFeature">功能特性类型.</typeparam>
    /// <returns>功能特性实例，如果插件不支持该功能则返回 null.</returns>
    TFeature? GetFeature<TFeature>()
        where TFeature : class, IPluginFeature;

    /// <summary>
    /// 获取插件提供的所有功能特性.
    /// </summary>
    /// <returns>功能特性列表.</returns>
    IReadOnlyList<IPluginFeature> GetAllFeatures();
}

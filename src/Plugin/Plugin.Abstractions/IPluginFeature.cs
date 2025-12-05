// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Abstractions;

/// <summary>
/// 插件功能特性基础接口.
/// 所有具体功能（如刮削器、书籍源等）都应继承此接口.
/// </summary>
public interface IPluginFeature
{
    /// <summary>
    /// 功能特性的唯一标识符.
    /// </summary>
    string FeatureId { get; }

    /// <summary>
    /// 功能特性的显示名称.
    /// </summary>
    string FeatureName { get; }
}

// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Abstractions;

/// <summary>
/// 插件加载结果.
/// </summary>
public sealed class PluginLoadResult
{
    private PluginLoadResult(bool success, IPlugin? plugin, string? errorMessage, Exception? exception)
    {
        Success = success;
        Plugin = plugin;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    /// <summary>
    /// 获取是否加载成功.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// 获取加载的插件实例.
    /// 仅在 <see cref="Success"/> 为 true 时有值.
    /// </summary>
    public IPlugin? Plugin { get; }

    /// <summary>
    /// 获取错误信息.
    /// 仅在 <see cref="Success"/> 为 false 时有值.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// 获取导致加载失败的异常.
    /// 仅在 <see cref="Success"/> 为 false 时可能有值.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// 创建成功结果.
    /// </summary>
    /// <param name="plugin">加载的插件.</param>
    /// <returns>成功结果.</returns>
    public static PluginLoadResult Succeeded(IPlugin plugin)
        => new(true, plugin, null, null);

    /// <summary>
    /// 创建失败结果.
    /// </summary>
    /// <param name="errorMessage">错误信息.</param>
    /// <param name="exception">异常.</param>
    /// <returns>失败结果.</returns>
    public static PluginLoadResult Failed(string errorMessage, Exception? exception = null)
        => new(false, null, errorMessage, exception);
}

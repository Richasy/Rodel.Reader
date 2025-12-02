// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.DownloadKit.Models;

/// <summary>
/// 下载配置选项.
/// </summary>
public sealed class DownloadOptions
{
    /// <summary>
    /// 默认缓冲区大小（80KB）.
    /// </summary>
    public const int DefaultBufferSize = 81920;

    /// <summary>
    /// 默认进度报告节流时间（毫秒）.
    /// </summary>
    public const int DefaultProgressThrottleMs = 100;

    /// <summary>
    /// 获取或设置自定义请求头.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// 获取或设置缓冲区大小（字节）.
    /// </summary>
    /// <remarks>
    /// 默认值为 81920 字节（80KB）.
    /// </remarks>
    public int BufferSize { get; set; } = DefaultBufferSize;

    /// <summary>
    /// 获取或设置请求超时时间.
    /// </summary>
    /// <remarks>
    /// 默认值为 30 秒.
    /// </remarks>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 获取或设置是否覆盖已存在的文件.
    /// </summary>
    /// <remarks>
    /// 默认值为 false，如果文件已存在则抛出异常.
    /// </remarks>
    public bool OverwriteExisting { get; set; }

    /// <summary>
    /// 获取或设置进度报告节流时间（毫秒）.
    /// </summary>
    /// <remarks>
    /// 默认值为 100 毫秒，用于防止进度报告过于频繁.
    /// </remarks>
    public int ProgressThrottleMs { get; set; } = DefaultProgressThrottleMs;

    /// <summary>
    /// 获取或设置用户代理字符串.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 创建默认选项.
    /// </summary>
    /// <returns>默认配置的 <see cref="DownloadOptions"/> 实例.</returns>
    public static DownloadOptions Default => new();
}

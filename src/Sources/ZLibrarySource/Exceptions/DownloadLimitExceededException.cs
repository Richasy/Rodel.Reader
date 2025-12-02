// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Exceptions;

/// <summary>
/// 下载限制超出异常.
/// </summary>
public sealed class DownloadLimitExceededException : ZLibraryException
{
    /// <summary>
    /// 初始化 <see cref="DownloadLimitExceededException"/> 类的新实例.
    /// </summary>
    public DownloadLimitExceededException()
        : base("Daily download limit has been exceeded.")
    {
    }

    /// <summary>
    /// 初始化 <see cref="DownloadLimitExceededException"/> 类的新实例.
    /// </summary>
    /// <param name="message">异常消息.</param>
    public DownloadLimitExceededException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="DownloadLimitExceededException"/> 类的新实例.
    /// </summary>
    /// <param name="dailyUsed">已使用次数.</param>
    /// <param name="dailyAllowed">允许次数.</param>
    public DownloadLimitExceededException(int dailyUsed, int dailyAllowed)
        : base($"Daily download limit exceeded. Used: {dailyUsed}, Allowed: {dailyAllowed}.")
    {
        DailyUsed = dailyUsed;
        DailyAllowed = dailyAllowed;
    }

    /// <summary>
    /// 获取已使用的下载次数.
    /// </summary>
    public int DailyUsed { get; }

    /// <summary>
    /// 获取允许的下载次数.
    /// </summary>
    public int DailyAllowed { get; }
}

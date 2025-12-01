// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// 无效的 Feed 格式异常.
/// </summary>
/// <remarks>
/// 当 Feed 内容不符合预期的格式（RSS/Atom）时抛出.
/// </remarks>
public class InvalidFeedFormatException : FeedParseException
{
    /// <summary>
    /// 初始化 <see cref="InvalidFeedFormatException"/> 类的新实例.
    /// </summary>
    public InvalidFeedFormatException()
        : base("无效的 Feed 格式.")
    {
    }

    /// <summary>
    /// 初始化 <see cref="InvalidFeedFormatException"/> 类的新实例.
    /// </summary>
    /// <param name="message">错误消息.</param>
    public InvalidFeedFormatException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="InvalidFeedFormatException"/> 类的新实例.
    /// </summary>
    /// <param name="message">错误消息.</param>
    /// <param name="innerException">内部异常.</param>
    public InvalidFeedFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// 初始化 <see cref="InvalidFeedFormatException"/> 类的新实例.
    /// </summary>
    /// <param name="expectedFormat">期望的格式.</param>
    /// <param name="actualContent">实际内容片段.</param>
    public InvalidFeedFormatException(FeedType expectedFormat, string? actualContent = null)
        : base($"期望 {expectedFormat} 格式的 Feed，但内容不匹配.")
    {
        ExpectedFormat = expectedFormat;
        ActualContent = actualContent;
    }

    /// <summary>
    /// 获取期望的 Feed 格式.
    /// </summary>
    public FeedType ExpectedFormat { get; init; }

    /// <summary>
    /// 获取实际的内容片段（用于诊断）.
    /// </summary>
    public string? ActualContent { get; init; }
}

// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// Feed 解析异常.
/// </summary>
/// <remarks>
/// 当解析 Feed 内容时发生错误时抛出.
/// </remarks>
public class FeedParseException : Exception
{
    /// <summary>
    /// 初始化 <see cref="FeedParseException"/> 类的新实例.
    /// </summary>
    public FeedParseException()
    {
    }

    /// <summary>
    /// 初始化 <see cref="FeedParseException"/> 类的新实例.
    /// </summary>
    /// <param name="message">错误消息.</param>
    public FeedParseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="FeedParseException"/> 类的新实例.
    /// </summary>
    /// <param name="message">错误消息.</param>
    /// <param name="innerException">内部异常.</param>
    public FeedParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// 获取或设置解析失败的元素名称.
    /// </summary>
    public string? ElementName { get; init; }

    /// <summary>
    /// 获取或设置解析失败时的行号.
    /// </summary>
    public int? LineNumber { get; init; }

    /// <summary>
    /// 获取或设置解析失败时的列号.
    /// </summary>
    public int? LinePosition { get; init; }
}

// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.FeedParser;

/// <summary>
/// 不支持的 Feed 格式异常.
/// </summary>
/// <remarks>
/// 当遇到无法识别的 Feed 格式时抛出.
/// </remarks>
public class UnsupportedFeedFormatException : FeedParseException
{
    /// <summary>
    /// 初始化 <see cref="UnsupportedFeedFormatException"/> 类的新实例.
    /// </summary>
    public UnsupportedFeedFormatException()
        : base("不支持的 Feed 格式.")
    {
    }

    /// <summary>
    /// 初始化 <see cref="UnsupportedFeedFormatException"/> 类的新实例.
    /// </summary>
    /// <param name="message">错误消息.</param>
    public UnsupportedFeedFormatException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="UnsupportedFeedFormatException"/> 类的新实例.
    /// </summary>
    /// <param name="message">错误消息.</param>
    /// <param name="innerException">内部异常.</param>
    public UnsupportedFeedFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// 初始化 <see cref="UnsupportedFeedFormatException"/> 类的新实例.
    /// </summary>
    /// <param name="rootElement">检测到的根元素名称.</param>
    /// <param name="namespaceUri">检测到的命名空间.</param>
    public UnsupportedFeedFormatException(string rootElement, string? namespaceUri)
        : base($"不支持的 Feed 格式: 根元素 '{rootElement}'" + 
               (string.IsNullOrEmpty(namespaceUri) ? "" : $", 命名空间 '{namespaceUri}'"))
    {
        DetectedRootElement = rootElement;
        DetectedNamespace = namespaceUri;
    }

    /// <summary>
    /// 获取检测到的根元素名称.
    /// </summary>
    public string? DetectedRootElement { get; init; }

    /// <summary>
    /// 获取检测到的命名空间.
    /// </summary>
    public string? DetectedNamespace { get; init; }
}

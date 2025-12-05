// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.FeedParser;

/// <summary>
/// Feed 格式化器接口.
/// </summary>
/// <remarks>
/// 负责将 Feed 模型对象格式化为 XML 字符串.
/// </remarks>
public interface IFeedFormatter
{
    /// <summary>
    /// 格式化频道信息.
    /// </summary>
    /// <param name="channel">频道信息.</param>
    /// <returns>XML 字符串.</returns>
    string FormatChannel(FeedChannel channel);

    /// <summary>
    /// 格式化订阅项.
    /// </summary>
    /// <param name="item">订阅项.</param>
    /// <returns>XML 字符串.</returns>
    string FormatItem(FeedItem item);

    /// <summary>
    /// 格式化链接.
    /// </summary>
    /// <param name="link">链接.</param>
    /// <returns>XML 字符串.</returns>
    string FormatLink(FeedLink link);

    /// <summary>
    /// 格式化人员.
    /// </summary>
    /// <param name="person">人员.</param>
    /// <returns>XML 字符串.</returns>
    string FormatPerson(FeedPerson person);

    /// <summary>
    /// 格式化分类.
    /// </summary>
    /// <param name="category">分类.</param>
    /// <returns>XML 字符串.</returns>
    string FormatCategory(FeedCategory category);

    /// <summary>
    /// 格式化图片.
    /// </summary>
    /// <param name="image">图片.</param>
    /// <returns>XML 字符串.</returns>
    string FormatImage(FeedImage image);

    /// <summary>
    /// 格式化原始内容.
    /// </summary>
    /// <param name="content">原始内容.</param>
    /// <returns>XML 字符串.</returns>
    string FormatContent(FeedContent content);

    /// <summary>
    /// 格式化值.
    /// </summary>
    /// <typeparam name="T">值类型.</typeparam>
    /// <param name="value">值.</param>
    /// <returns>格式化后的字符串.</returns>
    string? FormatValue<T>(T value);
}

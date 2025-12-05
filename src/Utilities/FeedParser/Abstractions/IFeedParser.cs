// Copyright (c) Reader Copilot. All rights reserved.

using System.Xml;

namespace Richasy.RodelReader.Utilities.FeedParser;

/// <summary>
/// Feed 解析器接口.
/// </summary>
/// <remarks>
/// 负责将 XML 内容解析为 Feed 模型对象.
/// </remarks>
public interface IFeedParser
{
    /// <summary>
    /// 解析频道信息.
    /// </summary>
    /// <param name="reader">XML 读取器.</param>
    /// <returns>频道信息.</returns>
    FeedChannel ParseChannel(XmlReader reader);

    /// <summary>
    /// 解析单个订阅项.
    /// </summary>
    /// <param name="content">原始内容.</param>
    /// <returns>订阅项.</returns>
    FeedItem ParseItem(FeedContent content);

    /// <summary>
    /// 解析链接.
    /// </summary>
    /// <param name="content">原始内容.</param>
    /// <returns>链接.</returns>
    FeedLink ParseLink(FeedContent content);

    /// <summary>
    /// 解析人员.
    /// </summary>
    /// <param name="content">原始内容.</param>
    /// <returns>人员.</returns>
    FeedPerson ParsePerson(FeedContent content);

    /// <summary>
    /// 解析分类.
    /// </summary>
    /// <param name="content">原始内容.</param>
    /// <returns>分类.</returns>
    FeedCategory ParseCategory(FeedContent content);

    /// <summary>
    /// 解析图片.
    /// </summary>
    /// <param name="content">原始内容.</param>
    /// <returns>图片.</returns>
    FeedImage ParseImage(FeedContent content);

    /// <summary>
    /// 解析原始内容.
    /// </summary>
    /// <param name="xml">XML 字符串.</param>
    /// <returns>原始内容对象.</returns>
    FeedContent ParseContent(string xml);

    /// <summary>
    /// 尝试解析值.
    /// </summary>
    /// <typeparam name="T">目标类型.</typeparam>
    /// <param name="value">字符串值.</param>
    /// <param name="result">解析结果.</param>
    /// <returns>是否解析成功.</returns>
    bool TryParseValue<T>(string? value, out T? result);
}

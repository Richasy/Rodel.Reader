// Copyright (c) Reader Copilot. All rights reserved.

using System.Xml;
using Richasy.RodelPlayer.Utilities.FeedParser.Internal;
using Richasy.RodelPlayer.Utilities.FeedParser.Parsers;

namespace Richasy.RodelPlayer.Utilities.FeedParser.Readers;

/// <summary>
/// Feed 读取器门面类.
/// </summary>
/// <remarks>
/// 提供自动检测 Feed 类型并创建相应读取器的功能.
/// </remarks>
public static class FeedReader
{
    /// <summary>
    /// 从流创建 Feed 读取器（自动检测类型）.
    /// </summary>
    /// <param name="stream">输入流.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>Feed 读取器.</returns>
    /// <exception cref="UnsupportedFeedFormatException">当无法识别 Feed 格式时抛出.</exception>
    public static async Task<IFeedReader> CreateAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // 读取流的前几个字节以检测格式
        var feedType = await DetectFeedTypeAsync(stream, cancellationToken).ConfigureAwait(false);

        // 重置流位置
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return feedType switch
        {
            FeedType.Rss => new RssFeedReader(stream),
            FeedType.Atom => new AtomFeedReader(stream),
            _ => throw new UnsupportedFeedFormatException("无法识别的 Feed 格式"),
        };
    }

    /// <summary>
    /// 从流创建 Feed 读取器（自动检测类型），使用自定义依赖.
    /// </summary>
    /// <param name="stream">输入流.</param>
    /// <param name="xmlReaderFactory">XML 读取器工厂.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>Feed 读取器.</returns>
    public static async Task<IFeedReader> CreateAsync(
        Stream stream,
        IXmlReaderFactory xmlReaderFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(xmlReaderFactory);

        var feedType = await DetectFeedTypeAsync(stream, cancellationToken).ConfigureAwait(false);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return feedType switch
        {
            FeedType.Rss => new RssFeedReader(stream, new RssParser(xmlReaderFactory), xmlReaderFactory),
            FeedType.Atom => new AtomFeedReader(stream, new AtomParser(xmlReaderFactory), xmlReaderFactory),
            _ => throw new UnsupportedFeedFormatException("无法识别的 Feed 格式"),
        };
    }

    /// <summary>
    /// 创建 RSS 读取器.
    /// </summary>
    /// <param name="stream">输入流.</param>
    /// <returns>RSS 读取器.</returns>
    public static IFeedReader CreateRssReader(Stream stream)
        => new RssFeedReader(stream);

    /// <summary>
    /// 创建 Atom 读取器.
    /// </summary>
    /// <param name="stream">输入流.</param>
    /// <returns>Atom 读取器.</returns>
    public static IFeedReader CreateAtomReader(Stream stream)
        => new AtomFeedReader(stream);

    /// <summary>
    /// 检测 Feed 类型.
    /// </summary>
    /// <param name="stream">输入流.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>Feed 类型.</returns>
    public static async Task<FeedType> DetectFeedTypeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // 使用包装流来跳过前导空白字符
        var wrappedStream = new LeadingWhitespaceSkippingStream(stream);

        var settings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Ignore,
            IgnoreComments = true,
            IgnoreWhitespace = true,
        };

        using var reader = XmlReader.Create(wrappedStream, settings);

        // 读取到第一个元素
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType == XmlNodeType.Element)
            {
                // 检测 RSS
                if (reader.LocalName == RssElementNames.Rss)
                {
                    return FeedType.Rss;
                }

                // 检测 Atom
                if (reader.LocalName == AtomElementNames.Feed &&
                    reader.NamespaceURI == AtomConstants.Atom10Namespace)
                {
                    return FeedType.Atom;
                }

                // 检测 RSS 1.0（RDF）
                if (reader.LocalName == "RDF")
                {
                    // RSS 1.0 暂不支持，但可以识别
                    return FeedType.Unknown;
                }

                // 无法识别
                return FeedType.Unknown;
            }
        }

        return FeedType.Unknown;
    }

    /// <summary>
    /// 快速读取 Feed 的所有内容.
    /// </summary>
    /// <param name="stream">输入流.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>频道信息和订阅项列表.</returns>
    public static async Task<(FeedChannel Channel, IReadOnlyList<FeedItem> Items)> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        using var reader = await CreateAsync(stream, cancellationToken).ConfigureAwait(false);

        var channel = await reader.ReadChannelAsync(cancellationToken).ConfigureAwait(false);
        var items = await reader.ReadAllItemsAsync(cancellationToken).ConfigureAwait(false);

        return (channel, items);
    }
}

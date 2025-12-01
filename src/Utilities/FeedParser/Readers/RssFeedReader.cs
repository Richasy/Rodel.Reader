// Copyright (c) Reader Copilot. All rights reserved.

using System.Runtime.CompilerServices;
using System.Xml;
using Richasy.RodelPlayer.Utilities.FeedParser.Internal;
using Richasy.RodelPlayer.Utilities.FeedParser.Parsers;

namespace Richasy.RodelPlayer.Utilities.FeedParser.Readers;

/// <summary>
/// RSS Feed 读取器.
/// </summary>
public sealed class RssFeedReader : IFeedReader, IDisposable
{
    private readonly XmlReader _reader;
    private readonly IFeedParser _parser;
    private readonly bool _ownsReader;
    private bool _disposed;
    private bool _initialized;
    private FeedChannel? _channel;

    /// <summary>
    /// 初始化 <see cref="RssFeedReader"/> 类的新实例.
    /// </summary>
    /// <param name="stream">输入流.</param>
    public RssFeedReader(Stream stream)
        : this(stream, new RssParser(), new XmlReaderFactory())
    {
    }

    /// <summary>
    /// 初始化 <see cref="RssFeedReader"/> 类的新实例（依赖注入）.
    /// </summary>
    /// <param name="stream">输入流.</param>
    /// <param name="parser">解析器.</param>
    /// <param name="xmlReaderFactory">XML 读取器工厂.</param>
    public RssFeedReader(Stream stream, IFeedParser parser, IXmlReaderFactory xmlReaderFactory)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(xmlReaderFactory);

        _reader = xmlReaderFactory.CreateReader(stream, async: true);
        _parser = parser;
        _ownsReader = true;
    }

    /// <summary>
    /// 初始化 <see cref="RssFeedReader"/> 类的新实例.
    /// </summary>
    /// <param name="reader">XML 读取器.</param>
    /// <param name="parser">解析器.</param>
    public RssFeedReader(XmlReader reader, IFeedParser parser)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _ownsReader = false;
    }

    /// <inheritdoc/>
    public FeedType FeedType => FeedType.Rss;

    /// <inheritdoc/>
    public async Task<FeedChannel> ReadChannelAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_channel != null)
        {
            return _channel;
        }

        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        _channel = _parser.ParseChannel(_reader);
        return _channel;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<FeedItem> ReadItemsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 确保已读取频道信息
        await ReadChannelAsync(cancellationToken).ConfigureAwait(false);

        // 继续读取 item 元素
        while (!_reader.EOF)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_reader.NodeType == XmlNodeType.Element &&
                _reader.LocalName == RssElementNames.Item)
            {
                var itemXml = await _reader.ReadOuterXmlAsync().ConfigureAwait(false);
                var content = _parser.ParseContent(itemXml);
                var item = _parser.ParseItem(content);
                yield return item;
            }
            else
            {
                await ReadNextAsync().ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FeedItem>> ReadAllItemsAsync(CancellationToken cancellationToken = default)
    {
        var items = new List<FeedItem>();

        await foreach (var item in ReadItemsAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(item);
        }

        return items;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_ownsReader)
        {
            _reader.Dispose();
        }

        _disposed = true;
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        // 读取到根元素
        while (await ReadNextAsync().ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_reader.NodeType == XmlNodeType.Element)
            {
                break;
            }
        }

        // 验证是否为 RSS 格式
        if (_reader.LocalName != RssElementNames.Rss)
        {
            throw new InvalidFeedFormatException(FeedType.Rss, _reader.LocalName);
        }

        var version = _reader.GetAttribute(RssElementNames.Version);
        if (version != RssConstants.Version)
        {
            throw new InvalidFeedFormatException($"不支持的 RSS 版本: {version}");
        }

        // 读取到 channel 元素
        while (await ReadNextAsync().ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_reader.NodeType == XmlNodeType.Element &&
                _reader.LocalName == RssElementNames.Channel)
            {
                // 进入 channel 元素
                await ReadNextAsync().ConfigureAwait(false);
                break;
            }
        }

        _initialized = true;
    }

    private Task<bool> ReadNextAsync()
    {
        // 根据 XmlReader 配置决定使用同步或异步读取
#pragma warning disable CA1849, VSTHRD103
        return _reader.Settings?.Async == true
            ? _reader.ReadAsync()
            : Task.FromResult(_reader.Read());
#pragma warning restore CA1849, VSTHRD103
    }
}

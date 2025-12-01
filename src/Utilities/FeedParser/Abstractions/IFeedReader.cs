// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// Feed 读取器接口.
/// </summary>
/// <remarks>
/// 提供读取 RSS/Atom 订阅源的统一接口.
/// </remarks>
public interface IFeedReader : IDisposable
{
    /// <summary>
    /// 获取 Feed 类型.
    /// </summary>
    FeedType FeedType { get; }

    /// <summary>
    /// 异步读取频道信息.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>频道信息.</returns>
    Task<FeedChannel> ReadChannelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步迭代读取所有订阅项.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>订阅项的异步枚举.</returns>
    IAsyncEnumerable<FeedItem> ReadItemsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步读取所有订阅项到列表.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>订阅项列表.</returns>
    Task<IReadOnlyList<FeedItem>> ReadAllItemsAsync(CancellationToken cancellationToken = default);
}

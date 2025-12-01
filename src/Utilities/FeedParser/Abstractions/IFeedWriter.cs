// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// Feed 写入器接口.
/// </summary>
/// <remarks>
/// 提供将 Feed 模型写入为 XML 的功能.
/// </remarks>
public interface IFeedWriter : IAsyncDisposable
{
    /// <summary>
    /// 写入频道信息.
    /// </summary>
    /// <param name="channel">频道信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task WriteChannelAsync(FeedChannel channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入订阅项.
    /// </summary>
    /// <param name="item">订阅项.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task WriteItemAsync(FeedItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入链接.
    /// </summary>
    /// <param name="link">链接.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task WriteLinkAsync(FeedLink link, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入人员.
    /// </summary>
    /// <param name="person">人员.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task WritePersonAsync(FeedPerson person, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入分类.
    /// </summary>
    /// <param name="category">分类.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task WriteCategoryAsync(FeedCategory category, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入图片.
    /// </summary>
    /// <param name="image">图片.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task WriteImageAsync(FeedImage image, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入原始内容.
    /// </summary>
    /// <param name="content">原始内容.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task WriteContentAsync(FeedContent content, CancellationToken cancellationToken = default);

    /// <summary>
    /// 刷新写入缓冲区.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    Task FlushAsync(CancellationToken cancellationToken = default);
}

// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService;

/// <summary>
/// OPDS 解析器接口.
/// </summary>
internal interface IOpdsParser
{
    /// <summary>
    /// 从流中解析 OPDS Feed.
    /// </summary>
    /// <param name="stream">XML 流.</param>
    /// <param name="baseUri">基础 URI，用于解析相对链接.</param>
    /// <returns>解析后的 OPDS Feed.</returns>
    OpdsFeed ParseFeed(Stream stream, Uri baseUri);

    /// <summary>
    /// 异步从流中解析 OPDS Feed.
    /// </summary>
    /// <param name="stream">XML 流.</param>
    /// <param name="baseUri">基础 URI，用于解析相对链接.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>解析后的 OPDS Feed.</returns>
    Task<OpdsFeed> ParseFeedAsync(Stream stream, Uri baseUri, CancellationToken cancellationToken = default);

    /// <summary>
    /// 解析 OpenSearch 描述文档，提取搜索模板.
    /// </summary>
    /// <param name="stream">OpenSearch 描述文档流.</param>
    /// <returns>搜索模板 URL.</returns>
    string? ParseOpenSearchDescription(Stream stream);

    /// <summary>
    /// 异步解析 OpenSearch 描述文档.
    /// </summary>
    /// <param name="stream">OpenSearch 描述文档流.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>搜索模板 URL.</returns>
    Task<string?> ParseOpenSearchDescriptionAsync(Stream stream, CancellationToken cancellationToken = default);
}

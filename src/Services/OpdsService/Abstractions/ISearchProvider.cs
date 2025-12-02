// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService;

/// <summary>
/// 搜索提供器接口.
/// </summary>
public interface ISearchProvider
{
    /// <summary>
    /// 从 Feed 中获取 OpenSearch 描述文档的 URL.
    /// </summary>
    /// <param name="feed">当前 Feed.</param>
    /// <returns>OpenSearch 描述文档 URL，如果不支持搜索则返回 null.</returns>
    Uri? GetSearchDescriptionUri(OpdsFeed feed);

    /// <summary>
    /// 获取搜索模板.
    /// </summary>
    /// <param name="feed">当前 Feed.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>搜索模板 URL，如果不支持搜索则返回 null.</returns>
    Task<string?> GetSearchTemplateAsync(OpdsFeed feed, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据搜索模板和查询词构建搜索 URL.
    /// </summary>
    /// <param name="searchTemplate">搜索模板（含 {searchTerms} 占位符）.</param>
    /// <param name="query">搜索关键词.</param>
    /// <returns>构建好的搜索 URL.</returns>
    Uri BuildSearchUri(string searchTemplate, string query);

    /// <summary>
    /// 执行搜索.
    /// </summary>
    /// <param name="searchTemplate">搜索模板.</param>
    /// <param name="query">搜索关键词.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>搜索结果 OPDS Feed.</returns>
    Task<OpdsFeed> SearchAsync(string searchTemplate, string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// 直接使用 Feed 执行搜索（自动获取搜索模板）.
    /// </summary>
    /// <param name="feed">当前 Feed.</param>
    /// <param name="query">搜索关键词.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>搜索结果 OPDS Feed.</returns>
    /// <exception cref="OpdsException">当 Feed 不支持搜索时抛出.</exception>
    Task<OpdsFeed> SearchAsync(OpdsFeed feed, string query, CancellationToken cancellationToken = default);
}

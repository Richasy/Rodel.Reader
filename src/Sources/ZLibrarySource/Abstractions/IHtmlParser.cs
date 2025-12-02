// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary;

/// <summary>
/// HTML 解析器接口.
/// </summary>
internal interface IHtmlParser
{
    /// <summary>
    /// 解析搜索结果页面.
    /// </summary>
    /// <param name="html">HTML 内容.</param>
    /// <param name="mirror">镜像地址.</param>
    /// <returns>书籍列表和总页数.</returns>
    (List<BookItem> Books, int TotalPages) ParseSearchResults(string html, string mirror);

    /// <summary>
    /// 解析书籍详情页面.
    /// </summary>
    /// <param name="html">HTML 内容.</param>
    /// <param name="url">页面 URL.</param>
    /// <param name="mirror">镜像地址.</param>
    /// <returns>书籍详情.</returns>
    BookDetail ParseBookDetail(string html, string url, string mirror);

    /// <summary>
    /// 解析下载限制页面.
    /// </summary>
    /// <param name="html">HTML 内容.</param>
    /// <returns>下载限制信息.</returns>
    DownloadLimits ParseDownloadLimits(string html);

    /// <summary>
    /// 解析下载历史页面.
    /// </summary>
    /// <param name="html">HTML 内容.</param>
    /// <param name="mirror">镜像地址.</param>
    /// <returns>下载历史列表.</returns>
    List<DownloadHistoryItem> ParseDownloadHistory(string html, string mirror);

    /// <summary>
    /// 解析书单列表页面.
    /// </summary>
    /// <param name="html">HTML 内容.</param>
    /// <param name="mirror">镜像地址.</param>
    /// <returns>书单列表和总页数.</returns>
    (List<Booklist> Booklists, int TotalPages) ParseBooklistResults(string html, string mirror);
}

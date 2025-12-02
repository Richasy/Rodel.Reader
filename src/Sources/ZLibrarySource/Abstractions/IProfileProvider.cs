// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary;

/// <summary>
/// 用户配置提供器接口.
/// </summary>
public interface IProfileProvider
{
    /// <summary>
    /// 获取每日下载限制信息.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>下载限制信息.</returns>
    Task<DownloadLimits> GetDownloadLimitsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取下载历史.
    /// </summary>
    /// <param name="page">页码（从 1 开始）.</param>
    /// <param name="fromDate">起始日期.</param>
    /// <param name="toDate">结束日期.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>下载历史分页结果.</returns>
    Task<PagedResult<DownloadHistoryItem>> GetDownloadHistoryAsync(
        int page = 1,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default);
}

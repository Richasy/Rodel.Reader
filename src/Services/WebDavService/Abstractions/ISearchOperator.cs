// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 搜索操作接口.
/// </summary>
public interface ISearchOperator
{
    /// <summary>
    /// 执行搜索操作.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="parameters">搜索参数.</param>
    /// <returns>PROPFIND 响应（包含搜索结果）.</returns>
    Task<PropfindResponse> SearchAsync(Uri requestUri, SearchParameters parameters);
}

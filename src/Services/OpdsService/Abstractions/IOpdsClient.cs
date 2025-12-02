// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService;

/// <summary>
/// OPDS 客户端接口.
/// </summary>
public interface IOpdsClient : IDisposable
{
    /// <summary>
    /// 获取目录导航器.
    /// </summary>
    ICatalogNavigator Catalog { get; }

    /// <summary>
    /// 获取搜索提供器.
    /// </summary>
    ISearchProvider Search { get; }

    /// <summary>
    /// 获取客户端配置.
    /// </summary>
    OpdsClientOptions Options { get; }
}

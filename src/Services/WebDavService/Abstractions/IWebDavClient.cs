// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 客户端接口.
/// </summary>
public interface IWebDavClient : IDisposable
{
    /// <summary>
    /// 获取属性操作器.
    /// </summary>
    IPropertyOperator Properties { get; }

    /// <summary>
    /// 获取资源操作器.
    /// </summary>
    IResourceOperator Resources { get; }

    /// <summary>
    /// 获取文件操作器.
    /// </summary>
    IFileOperator Files { get; }

    /// <summary>
    /// 获取锁操作器.
    /// </summary>
    ILockOperator Locks { get; }

    /// <summary>
    /// 获取搜索操作器.
    /// </summary>
    ISearchOperator Search { get; }
}

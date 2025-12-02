// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary;

/// <summary>
/// ZLibrary 客户端接口.
/// </summary>
public interface IZLibraryClient : IDisposable
{
    /// <summary>
    /// 获取搜索模块.
    /// </summary>
    ISearchProvider Search { get; }

    /// <summary>
    /// 获取书籍详情模块.
    /// </summary>
    IBookDetailProvider Books { get; }

    /// <summary>
    /// 获取用户配置模块.
    /// </summary>
    IProfileProvider Profile { get; }

    /// <summary>
    /// 获取书单模块.
    /// </summary>
    IBooklistProvider Booklists { get; }

    /// <summary>
    /// 获取客户端配置.
    /// </summary>
    ZLibraryClientOptions Options { get; }

    /// <summary>
    /// 获取是否已认证.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// 获取当前镜像地址.
    /// </summary>
    string Mirror { get; }

    /// <summary>
    /// 登录到 ZLibrary.
    /// </summary>
    /// <param name="email">邮箱地址.</param>
    /// <param name="password">密码.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>登录任务.</returns>
    Task LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// 登出.
    /// </summary>
    void Logout();
}

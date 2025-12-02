// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary;

/// <summary>
/// ZLibrary 客户端接口.
/// </summary>
public interface IZLibraryClient : IDisposable
{
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

    /// <summary>
    /// 搜索书籍.
    /// </summary>
    /// <param name="query">搜索关键词.</param>
    /// <param name="page">页码（从 1 开始）.</param>
    /// <param name="options">搜索选项.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>分页搜索结果.</returns>
    Task<PagedResult<BookItem>> SearchAsync(
        string query,
        int page = 1,
        BookSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户资料信息.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>用户资料信息.</returns>
    Task<UserProfile> GetProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取书籍的真实下载链接.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="bookHash">书籍哈希值.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>下载信息，如果不允许下载则返回 null.</returns>
    Task<DownloadInfo?> GetDownloadInfoAsync(string bookId, string bookHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取书籍的真实下载链接.
    /// </summary>
    /// <param name="book">书籍项.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>下载信息，如果不允许下载则返回 null.</returns>
    Task<DownloadInfo?> GetDownloadInfoAsync(BookItem book, CancellationToken cancellationToken = default);
}

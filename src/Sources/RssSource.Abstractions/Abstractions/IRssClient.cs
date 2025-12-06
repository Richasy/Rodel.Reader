// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 客户端接口.
/// 定义与 RSS 服务交互的完整能力，组合了读取、写入和 OPML 处理功能.
/// </summary>
public interface IRssClient : IRssReader, IRssWriter, IRssOpmlHandler, IDisposable
{
    /// <summary>
    /// 获取此 RSS 源的能力元数据.
    /// </summary>
    IRssSourceCapabilities Capabilities { get; }

    /// <summary>
    /// 获取当前是否已通过身份验证.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// 登录到 RSS 服务.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>登录是否成功.</returns>
    /// <remarks>
    /// 对于不需要认证的源（如本地源），此方法应直接返回 true.
    /// </remarks>
    Task<bool> SignInAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 从 RSS 服务登出.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>登出是否成功.</returns>
    /// <remarks>
    /// 对于不需要认证的源（如本地源），此方法应直接返回 true.
    /// </remarks>
    Task<bool> SignOutAsync(CancellationToken cancellationToken = default);
}

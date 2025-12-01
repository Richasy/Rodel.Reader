// Copyright (c) Richasy. All rights reserved.

using System.Net;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 客户端配置选项.
/// </summary>
public sealed class WebDavClientOptions
{
    /// <summary>
    /// 初始化 <see cref="WebDavClientOptions"/> 类的新实例.
    /// </summary>
    public WebDavClientOptions()
    {
        DefaultHeaders = new Dictionary<string, string>();
        UseDefaultCredentials = true;
        PreAuthenticate = true;
        UseProxy = true;
    }

    /// <summary>
    /// 获取或设置基础地址.
    /// </summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>
    /// 获取或设置凭据.
    /// </summary>
    public ICredentials? Credentials { get; set; }

    /// <summary>
    /// 获取或设置超时时间.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// 获取或设置是否使用默认凭据.
    /// </summary>
    public bool UseDefaultCredentials { get; set; }

    /// <summary>
    /// 获取或设置是否预认证.
    /// </summary>
    public bool PreAuthenticate { get; set; }

    /// <summary>
    /// 获取或设置是否使用代理.
    /// </summary>
    public bool UseProxy { get; set; }

    /// <summary>
    /// 获取或设置代理.
    /// </summary>
    public IWebProxy? Proxy { get; set; }

    /// <summary>
    /// 获取或设置默认请求头.
    /// </summary>
    public IDictionary<string, string> DefaultHeaders { get; set; }
}

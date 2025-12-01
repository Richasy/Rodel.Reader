// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// GET 文件操作参数.
/// </summary>
public sealed class GetFileParameters : RequestParameters
{
    /// <summary>
    /// 获取或设置是否允许服务器处理响应.
    /// </summary>
    /// <remarks>
    /// 设置为 true 时，服务器可能会处理响应（如执行脚本）。
    /// 设置为 false 时，服务器将返回原始文件内容。
    /// </remarks>
    public bool Translate { get; set; }
}

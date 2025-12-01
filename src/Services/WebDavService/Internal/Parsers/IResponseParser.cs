// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// 响应解析器接口.
/// </summary>
/// <typeparam name="T">响应类型.</typeparam>
internal interface IResponseParser<T>
    where T : WebDavResponse
{
    /// <summary>
    /// 解析响应.
    /// </summary>
    /// <param name="responseContent">响应内容.</param>
    /// <param name="statusCode">状态码.</param>
    /// <param name="description">描述.</param>
    /// <returns>解析后的响应.</returns>
    T Parse(string responseContent, int statusCode, string? description);
}

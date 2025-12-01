// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// If 请求头辅助类.
/// </summary>
internal static class IfHeaderHelper
{
    /// <summary>
    /// 获取 If 请求头值.
    /// </summary>
    /// <param name="lockToken">锁令牌.</param>
    /// <returns>If 请求头值.</returns>
    public static string GetHeaderValue(string lockToken)
    {
        return $"(<{lockToken}>)";
    }
}

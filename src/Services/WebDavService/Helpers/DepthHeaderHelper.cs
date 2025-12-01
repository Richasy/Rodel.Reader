// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// Depth 请求头辅助类.
/// </summary>
internal static class DepthHeaderHelper
{
    /// <summary>
    /// 获取 PROPFIND 操作的 Depth 值.
    /// </summary>
    /// <param name="applyTo">应用范围.</param>
    /// <returns>Depth 值.</returns>
    public static string GetValueForPropfind(PropfindApplyTo applyTo)
    {
        return applyTo.Depth switch
        {
            0 => "0",
            1 => "1",
            _ => "infinity",
        };
    }

    /// <summary>
    /// 获取 COPY 操作的 Depth 值.
    /// </summary>
    /// <param name="applyTo">应用范围.</param>
    /// <returns>Depth 值.</returns>
    public static string GetValueForCopy(CopyApplyTo applyTo)
    {
        return applyTo.Depth == 0 ? "0" : "infinity";
    }

    /// <summary>
    /// 获取 LOCK 操作的 Depth 值.
    /// </summary>
    /// <param name="applyTo">应用范围.</param>
    /// <returns>Depth 值.</returns>
    public static string GetValueForLock(LockApplyTo applyTo)
    {
        return applyTo.Depth == 0 ? "0" : "infinity";
    }
}

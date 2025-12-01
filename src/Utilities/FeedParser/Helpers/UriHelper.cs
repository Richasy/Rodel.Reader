// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser.Helpers;

/// <summary>
/// URI 解析帮助类.
/// </summary>
public static class UriHelper
{
    /// <summary>
    /// 尝试解析 URI 字符串.
    /// </summary>
    /// <param name="value">URI 字符串.</param>
    /// <param name="result">解析结果.</param>
    /// <returns>是否解析成功.</returns>
    public static bool TryParse(string? value, out Uri? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Uri.TryCreate(value.Trim(), UriKind.RelativeOrAbsolute, out result);
    }

    /// <summary>
    /// 尝试解析为绝对 URI.
    /// </summary>
    /// <param name="value">URI 字符串.</param>
    /// <param name="baseUri">基础 URI（用于解析相对路径）.</param>
    /// <param name="result">解析结果.</param>
    /// <returns>是否解析成功.</returns>
    public static bool TryParseAbsolute(string? value, Uri? baseUri, out Uri? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        value = value.Trim();

        // 尝试直接解析为绝对 URI
        if (Uri.TryCreate(value, UriKind.Absolute, out result))
        {
            return true;
        }

        // 尝试使用基础 URI 解析相对路径
        if (baseUri != null && Uri.TryCreate(baseUri, value, out result))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 规范化 URI 字符串.
    /// </summary>
    /// <param name="value">URI 字符串.</param>
    /// <returns>规范化后的 URI 字符串.</returns>
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();

        // 移除可能的空白字符
        value = value.Replace(" ", "%20", StringComparison.Ordinal);

        return value;
    }
}

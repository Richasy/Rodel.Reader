// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// 属性值解析器.
/// </summary>
internal static class PropertyValueParser
{
    /// <summary>
    /// 解析长整型值.
    /// </summary>
    public static long? ParseLong(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return long.TryParse(value, out var result) ? result : null;
    }

    /// <summary>
    /// 解析日期时间偏移值.
    /// </summary>
    public static DateTimeOffset? ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, out var result) ? result : null;
    }

    /// <summary>
    /// 解析布尔值.
    /// </summary>
    public static bool ParseBool(string? value, bool defaultValue = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return value.Equals("T", StringComparison.OrdinalIgnoreCase)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Equals("1", StringComparison.Ordinal);
    }

    /// <summary>
    /// 解析资源类型.
    /// </summary>
    public static bool IsCollection(XElement? resourceTypeElement)
    {
        if (resourceTypeElement == null)
        {
            return false;
        }

        return resourceTypeElement.LocalElement("collection") != null;
    }

    /// <summary>
    /// 从状态行解析状态码.
    /// </summary>
    /// <param name="statusLine">状态行，如 "HTTP/1.1 200 OK".</param>
    /// <returns>状态码.</returns>
    public static int ParseStatusCode(string? statusLine)
    {
        if (string.IsNullOrWhiteSpace(statusLine))
        {
            return 0;
        }

        var parts = statusLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && int.TryParse(parts[1], out var statusCode))
        {
            return statusCode;
        }

        return 0;
    }
}

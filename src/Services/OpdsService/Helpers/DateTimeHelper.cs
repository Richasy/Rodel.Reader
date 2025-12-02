// Copyright (c) Richasy. All rights reserved.

using System.Globalization;

namespace Richasy.RodelReader.Services.OpdsService.Helpers;

/// <summary>
/// 日期时间辅助类.
/// </summary>
internal static class DateTimeHelper
{
    private static readonly string[] DateFormats =
    [
        // ISO 8601 格式
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss.fZ",
        "yyyy-MM-ddTHH:mm:ss.ffZ",
        "yyyy-MM-ddTHH:mm:ss.fffZ",
        "yyyy-MM-ddTHH:mm:sszzz",
        "yyyy-MM-ddTHH:mm:ss.fzzz",
        "yyyy-MM-ddTHH:mm:ss.ffzzz",
        "yyyy-MM-ddTHH:mm:ss.fffzzz",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd",

        // RFC 822 格式
        "ddd, dd MMM yyyy HH:mm:ss zzz",
        "ddd, dd MMM yyyy HH:mm:ss Z",
        "ddd, dd MMM yyyy HH:mm:ss",
        "dd MMM yyyy HH:mm:ss zzz",
        "dd MMM yyyy HH:mm:ss",
    ];

    /// <summary>
    /// 尝试解析日期时间字符串.
    /// </summary>
    /// <param name="dateString">日期时间字符串.</param>
    /// <param name="result">解析结果.</param>
    /// <returns>是否解析成功.</returns>
    public static bool TryParseDate(string? dateString, out DateTimeOffset result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return false;
        }

        // 尝试标准解析
        if (DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
            return true;
        }

        // 尝试使用预定义格式
        if (DateTimeOffset.TryParseExact(dateString, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
            return true;
        }

        return false;
    }
}

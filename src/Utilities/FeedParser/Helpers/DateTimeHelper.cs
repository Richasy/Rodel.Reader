// Copyright (c) Reader Copilot. All rights reserved.

using System.Globalization;
using System.Text;

namespace Richasy.RodelPlayer.Utilities.FeedParser.Helpers;

/// <summary>
/// 日期时间解析帮助类.
/// </summary>
public static class DateTimeHelper
{
    private const string Rfc3339LocalDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
    private const string Rfc3339UtcDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

    /// <summary>
    /// 尝试解析日期时间字符串.
    /// </summary>
    /// <param name="value">日期时间字符串.</param>
    /// <param name="result">解析结果.</param>
    /// <returns>是否解析成功.</returns>
    public static bool TryParseDate(string? value, out DateTimeOffset result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            return false;
        }

        return TryParseDateRfc3339(value, out result)
            || TryParseDateRfc822(value, out result)
            || DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }

    /// <summary>
    /// 将 DateTimeOffset 转换为 RFC 3339 格式字符串.
    /// </summary>
    /// <param name="dateTime">日期时间.</param>
    /// <returns>RFC 3339 格式字符串.</returns>
    public static string ToRfc3339String(DateTimeOffset dateTime)
    {
        return dateTime.Offset == TimeSpan.Zero
            ? dateTime.ToUniversalTime().ToString(Rfc3339UtcDateTimeFormat, CultureInfo.InvariantCulture)
            : dateTime.ToString(Rfc3339LocalDateTimeFormat, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 将 DateTimeOffset 转换为 RFC 1123 格式字符串.
    /// </summary>
    /// <param name="dateTime">日期时间.</param>
    /// <returns>RFC 1123 格式字符串.</returns>
    public static string ToRfc1123String(DateTimeOffset dateTime)
        => dateTime.ToString("r", CultureInfo.InvariantCulture);

    /// <summary>
    /// 尝试解析时长字符串（如 "01:23:45" 或 "1234"）.
    /// </summary>
    /// <param name="value">时长字符串.</param>
    /// <param name="seconds">解析后的秒数.</param>
    /// <returns>是否解析成功.</returns>
    public static bool TryParseDuration(string? value, out int seconds)
    {
        seconds = 0;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        value = value.Trim();

        // 尝试解析纯数字（秒）
        if (int.TryParse(value, out seconds))
        {
            return true;
        }

        // 尝试解析 HH:MM:SS 或 MM:SS 格式
        var parts = value.Split(':');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out var minutes) &&
            int.TryParse(parts[1], out var secs))
        {
            seconds = (minutes * 60) + secs;
            return true;
        }

        if (parts.Length == 3 &&
            int.TryParse(parts[0], out var hours) &&
            int.TryParse(parts[1], out minutes) &&
            int.TryParse(parts[2], out secs))
        {
            seconds = (hours * 3600) + (minutes * 60) + secs;
            return true;
        }

        return false;
    }

    private static bool TryParseDateRfc3339(string value, out DateTimeOffset result)
    {
        // 移除毫秒部分
        value = RemoveFractionalSeconds(value);

        if (DateTimeOffset.TryParseExact(
            value,
            Rfc3339LocalDateTimeFormat,
            CultureInfo.InvariantCulture.DateTimeFormat,
            DateTimeStyles.None,
            out result))
        {
            return true;
        }

        if (DateTimeOffset.TryParseExact(
            value,
            Rfc3339UtcDateTimeFormat,
            CultureInfo.InvariantCulture.DateTimeFormat,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out result))
        {
            return true;
        }

        return false;
    }

    private static bool TryParseDateRfc822(string value, out DateTimeOffset result)
    {
        result = default;

        var sb = new StringBuilder(value.Trim());

        if (sb.Length < 18)
        {
            return false;
        }

        // 移除开头的星期几（如 "Tue, "）
        if (sb.Length > 3 && sb[3] == ',')
        {
            sb.Remove(0, 4);
            TrimStart(sb);
        }

        CollapseWhitespaces(sb);

        // 确保日期是两位数
        if (!char.IsDigit(sb[1]))
        {
            sb.Insert(0, '0');
        }

        if (sb.Length < 19)
        {
            return false;
        }

        var hasSeconds = sb[17] == ':';
        var timeZoneStartIndex = hasSeconds ? 21 : 18;

        if (sb.Length <= timeZoneStartIndex)
        {
            return false;
        }

        var timeZoneSuffix = sb.ToString()[timeZoneStartIndex..];
        sb.Remove(timeZoneStartIndex, sb.Length - timeZoneStartIndex);

        var normalizedTimeZone = NormalizeTimeZone(timeZoneSuffix, out var isUtc);
        sb.Append(normalizedTimeZone);

        var parseFormat = hasSeconds ? "dd MMM yyyy HH:mm:ss zzz" : "dd MMM yyyy HH:mm zzz";

        return DateTimeOffset.TryParseExact(
            sb.ToString(),
            parseFormat,
            CultureInfo.InvariantCulture.DateTimeFormat,
            isUtc ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None,
            out result);
    }

    private static string NormalizeTimeZone(string rfc822TimeZone, out bool isUtc)
    {
        isUtc = false;

        if (string.IsNullOrEmpty(rfc822TimeZone))
        {
            return "-00:00";
        }

        // 处理 +/-HHMM 格式
        if (rfc822TimeZone[0] is '+' or '-')
        {
            var result = new StringBuilder(rfc822TimeZone);
            if (result.Length == 4)
            {
                result.Insert(1, '0');
            }

            if (result.Length >= 5)
            {
                result.Insert(3, ':');
            }

            return result.ToString();
        }

        // 处理命名时区
        isUtc = rfc822TimeZone is "UT" or "Z" or "UTC";

        return rfc822TimeZone switch
        {
            "UT" or "Z" or "UTC" or "GMT" => "-00:00",
            "EDT" => "-04:00",
            "EST" or "CDT" => "-05:00",
            "CST" or "MDT" => "-06:00",
            "MST" or "PDT" => "-07:00",
            "PST" => "-08:00",
            _ => "-00:00",
        };
    }

    private static string RemoveFractionalSeconds(string value)
    {
        value = value.Trim();

        if (value.Length < 20)
        {
            return value;
        }

        var dotIndex = value.IndexOf('.', StringComparison.Ordinal);
        if (dotIndex < 0 || dotIndex != 19)
        {
            return value;
        }

        var i = 20;
        while (i < value.Length && char.IsDigit(value[i]))
        {
            i++;
        }

        return value[..19] + value[i..];
    }

    private static void TrimStart(StringBuilder sb)
    {
        var i = 0;
        while (i < sb.Length && char.IsWhiteSpace(sb[i]))
        {
            i++;
        }

        if (i > 0)
        {
            sb.Remove(0, i);
        }
    }

    private static void CollapseWhitespaces(StringBuilder sb)
    {
        var index = 0;
        var whiteSpaceStart = -1;

        while (index < sb.Length)
        {
            if (char.IsWhiteSpace(sb[index]))
            {
                if (whiteSpaceStart < 0)
                {
                    whiteSpaceStart = index;
                    sb[index] = ' ';
                }
            }
            else if (whiteSpaceStart >= 0)
            {
                if (index > whiteSpaceStart + 1)
                {
                    sb.Remove(whiteSpaceStart, index - whiteSpaceStart - 1);
                    index = whiteSpaceStart + 1;
                }

                whiteSpaceStart = -1;
            }

            index++;
        }
    }
}

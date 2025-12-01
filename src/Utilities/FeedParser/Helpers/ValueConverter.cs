// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser.Helpers;

/// <summary>
/// 类型转换帮助类.
/// </summary>
public static class ValueConverter
{
    /// <summary>
    /// 尝试将字符串转换为指定类型.
    /// </summary>
    /// <typeparam name="T">目标类型.</typeparam>
    /// <param name="value">字符串值.</param>
    /// <param name="result">转换结果.</param>
    /// <returns>是否转换成功.</returns>
    public static bool TryConvert<T>(string? value, out T? result)
    {
        result = default;
        var type = typeof(T);

        // 字符串类型
        if (type == typeof(string))
        {
            result = (T)(object)(value ?? string.Empty);
            return true;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // DateTimeOffset
        if (type == typeof(DateTimeOffset))
        {
            if (DateTimeHelper.TryParseDate(value, out var dateTime))
            {
                result = (T)(object)dateTime;
                return true;
            }

            return false;
        }

        // DateTime
        if (type == typeof(DateTime))
        {
            if (DateTimeHelper.TryParseDate(value, out var dateTime))
            {
                result = (T)(object)dateTime.DateTime;
                return true;
            }

            return false;
        }

        // Uri
        if (type == typeof(Uri))
        {
            if (UriHelper.TryParse(value, out var uri) && uri != null)
            {
                result = (T)(object)uri;
                return true;
            }

            return false;
        }

        // bool
        if (type == typeof(bool))
        {
            if (bool.TryParse(value, out var boolValue))
            {
                result = (T)(object)boolValue;
                return true;
            }

            // 处理 "yes"/"no" 和 "1"/"0"
            var normalized = value.Trim().ToLowerInvariant();
            if (normalized is "yes" or "1" or "true")
            {
                result = (T)(object)true;
                return true;
            }

            if (normalized is "no" or "0" or "false")
            {
                result = (T)(object)false;
                return true;
            }

            return false;
        }

        // int
        if (type == typeof(int))
        {
            if (int.TryParse(value, out var intValue))
            {
                result = (T)(object)intValue;
                return true;
            }

            return false;
        }

        // long
        if (type == typeof(long))
        {
            if (long.TryParse(value, out var longValue))
            {
                result = (T)(object)longValue;
                return true;
            }

            return false;
        }

        // double
        if (type == typeof(double))
        {
            if (double.TryParse(value, out var doubleValue))
            {
                result = (T)(object)doubleValue;
                return true;
            }

            return false;
        }

        // 回退到默认转换
        try
        {
            result = (T)Convert.ChangeType(value, typeof(T));
            return result != null;
        }
        catch
        {
            return false;
        }
    }
}

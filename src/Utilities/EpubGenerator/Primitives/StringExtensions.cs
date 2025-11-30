// Copyright (c) Reader Copilot. All rights reserved.

using System.Runtime.CompilerServices;

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 高性能字符串扩展方法.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// 替换字符串（使用 Ordinal 比较）.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReplaceOrdinal(this string value, string oldValue, string newValue)
        => value.Replace(oldValue, newValue, StringComparison.Ordinal);

    /// <summary>
    /// HTML 实体编码（高性能版本）.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string HtmlEncode(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return HtmlEncode(value.AsSpan());
    }

    /// <summary>
    /// HTML 实体编码（Span 版本）.
    /// </summary>
    public static string HtmlEncode(this ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            return string.Empty;
        }

        // 快速路径：检查是否需要编码
        var needsEncoding = false;
        foreach (var c in value)
        {
            if (c is '&' or '<' or '>' or '"' or '\'')
            {
                needsEncoding = true;
                break;
            }
        }

        if (!needsEncoding)
        {
            return value.ToString();
        }

        // 慢速路径：需要编码
        var sb = StringBuilderPool.Rent();
        foreach (var c in value)
        {
            _ = c switch
            {
                '&' => sb.Append("&amp;"),
                '<' => sb.Append("&lt;"),
                '>' => sb.Append("&gt;"),
                '"' => sb.Append("&quot;"),
                '\'' => sb.Append("&apos;"),
                _ => sb.Append(c),
            };
        }

        return StringBuilderPool.ToStringAndReturn(sb);
    }

    /// <summary>
    /// XML 实体编码.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string XmlEncode(this string value) => HtmlEncode(value);

    /// <summary>
    /// XML 实体编码（Span 版本）.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string XmlEncode(this ReadOnlySpan<char> value) => HtmlEncode(value);

    /// <summary>
    /// 检查字符串是否以任一前缀开头.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithAny(this ReadOnlySpan<char> value, ReadOnlySpan<string> prefixes)
    {
        foreach (var prefix in prefixes)
        {
            if (value.StartsWith(prefix.AsSpan(), StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 检查字符串是否以任一前缀开头.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithAny(this string value, ReadOnlySpan<string> prefixes)
        => value.AsSpan().StartsWithAny(prefixes);

    /// <summary>
    /// 修剪并检查是否为空.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTrimmedEmpty(this ReadOnlySpan<char> value)
        => value.Trim().IsEmpty;

    /// <summary>
    /// 统计行数（不分配内存）.
    /// </summary>
    public static int CountLines(this ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            return 0;
        }

        var count = 1;
        var index = 0;
        while ((index = value[index..].IndexOf('\n')) >= 0)
        {
            count++;
            index++;
            if (index >= value.Length)
            {
                break;
            }
        }

        return count;
    }
}

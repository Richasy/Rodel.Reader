// Copyright (c) Richasy. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// 参数验证辅助类.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// 确保参数不为 null.
    /// </summary>
    /// <typeparam name="T">参数类型.</typeparam>
    /// <param name="value">参数值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <exception cref="ArgumentNullException">当参数为 null 时抛出.</exception>
    public static void NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
    }

    /// <summary>
    /// 确保字符串不为 null 或空.
    /// </summary>
    /// <param name="value">字符串值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <exception cref="ArgumentException">当字符串为 null 或空时抛出.</exception>
    public static void NotNullOrEmpty([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);
    }

    /// <summary>
    /// 确保字符串不为 null、空或仅包含空白字符.
    /// </summary>
    /// <param name="value">字符串值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <exception cref="ArgumentException">当字符串为 null、空或仅包含空白字符时抛出.</exception>
    public static void NotNullOrWhiteSpace([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
    }
}

// Copyright (c) Richasy. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Richasy.RodelReader.Sources.Podcast.Apple.Helpers;

/// <summary>
/// 参数校验辅助类.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// 确保参数不为 null.
    /// </summary>
    /// <typeparam name="T">参数类型.</typeparam>
    /// <param name="value">参数值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <returns>非 null 的参数值.</returns>
    /// <exception cref="ArgumentNullException">当参数为 null 时抛出.</exception>
    public static T NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }

    /// <summary>
    /// 确保字符串不为 null 或空.
    /// </summary>
    /// <param name="value">字符串值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <returns>非空字符串.</returns>
    /// <exception cref="ArgumentException">当字符串为 null 或空时抛出.</exception>
    public static string NotNullOrEmpty([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);
        return value;
    }

    /// <summary>
    /// 确保字符串不为 null、空或仅包含空白字符.
    /// </summary>
    /// <param name="value">字符串值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <returns>非空白字符串.</returns>
    /// <exception cref="ArgumentException">当字符串为 null、空或仅包含空白字符时抛出.</exception>
    public static string NotNullOrWhiteSpace([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value;
    }
}

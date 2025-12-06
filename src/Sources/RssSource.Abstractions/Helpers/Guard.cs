// Copyright (c) Richasy. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Richasy.RodelReader.Sources.Rss.Abstractions.Helpers;

/// <summary>
/// 参数验证辅助类.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// 验证参数不为 null.
    /// </summary>
    /// <typeparam name="T">参数类型.</typeparam>
    /// <param name="value">参数值.</param>
    /// <param name="parameterName">参数名称.</param>
    /// <exception cref="ArgumentNullException">当参数为 null 时抛出.</exception>
    public static void NotNull<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }

    /// <summary>
    /// 验证字符串不为 null 或空.
    /// </summary>
    /// <param name="value">字符串值.</param>
    /// <param name="parameterName">参数名称.</param>
    /// <exception cref="ArgumentNullException">当字符串为 null 时抛出.</exception>
    /// <exception cref="ArgumentException">当字符串为空时抛出.</exception>
    public static void NotNullOrEmpty(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }
    }

    /// <summary>
    /// 验证字符串不为 null、空或仅含空白字符.
    /// </summary>
    /// <param name="value">字符串值.</param>
    /// <param name="parameterName">参数名称.</param>
    /// <exception cref="ArgumentNullException">当字符串为 null 时抛出.</exception>
    /// <exception cref="ArgumentException">当字符串为空或仅含空白字符时抛出.</exception>
    public static void NotNullOrWhiteSpace(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty or whitespace.", parameterName);
        }
    }

    /// <summary>
    /// 验证数值为非负数.
    /// </summary>
    /// <param name="value">数值.</param>
    /// <param name="parameterName">参数名称.</param>
    /// <exception cref="ArgumentOutOfRangeException">当数值为负数时抛出.</exception>
    public static void NonNegative(
        int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Value cannot be negative.");
        }
    }

    /// <summary>
    /// 验证数值为正数.
    /// </summary>
    /// <param name="value">数值.</param>
    /// <param name="parameterName">参数名称.</param>
    /// <exception cref="ArgumentOutOfRangeException">当数值不为正数时抛出.</exception>
    public static void Positive(
        int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be positive.");
        }
    }

    /// <summary>
    /// 验证集合不为空.
    /// </summary>
    /// <typeparam name="T">集合元素类型.</typeparam>
    /// <param name="collection">集合.</param>
    /// <param name="parameterName">参数名称.</param>
    /// <exception cref="ArgumentNullException">当集合为 null 时抛出.</exception>
    /// <exception cref="ArgumentException">当集合为空时抛出.</exception>
    public static void NotEmpty<T>(
        [NotNull] IEnumerable<T>? collection,
        [CallerArgumentExpression(nameof(collection))] string? parameterName = null)
    {
        NotNull(collection, parameterName);

        if (!collection.Any())
        {
            throw new ArgumentException("Collection cannot be empty.", parameterName);
        }
    }
}

// Copyright (c) Richasy. All rights reserved.

using System.Runtime.CompilerServices;

namespace Richasy.RodelReader.Sources.FanQie.Helpers;

/// <summary>
/// 参数检查辅助类.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// 确保字符串不为空.
    /// </summary>
    /// <param name="value">要检查的值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <exception cref="ArgumentNullException">值为 null 时抛出.</exception>
    /// <exception cref="ArgumentException">值为空字符串时抛出.</exception>
    public static void NotNullOrEmpty(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", paramName);
        }
    }

    /// <summary>
    /// 确保对象不为 null.
    /// </summary>
    /// <typeparam name="T">对象类型.</typeparam>
    /// <param name="value">要检查的值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <exception cref="ArgumentNullException">值为 null 时抛出.</exception>
    public static void NotNull<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    /// <summary>
    /// 确保数值大于零.
    /// </summary>
    /// <param name="value">要检查的值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <exception cref="ArgumentOutOfRangeException">值小于等于零时抛出.</exception>
    public static void Positive(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
        }
    }

    /// <summary>
    /// 确保数值非负.
    /// </summary>
    /// <param name="value">要检查的值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <exception cref="ArgumentOutOfRangeException">值小于零时抛出.</exception>
    public static void NonNegative(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Value cannot be negative.");
        }
    }
}

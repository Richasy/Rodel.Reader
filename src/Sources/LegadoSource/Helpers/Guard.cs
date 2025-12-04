// Copyright (c) Richasy. All rights reserved.

using System.Runtime.CompilerServices;

namespace Richasy.RodelReader.Sources.Legado.Helpers;

/// <summary>
/// 参数验证辅助类.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// 确保字符串不为空.
    /// </summary>
    /// <param name="value">要检查的值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <returns>原始值.</returns>
    /// <exception cref="ArgumentNullException">值为 null 时抛出.</exception>
    /// <exception cref="ArgumentException">值为空字符串时抛出.</exception>
    public static string NotNullOrEmpty(
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

        return value;
    }

    /// <summary>
    /// 确保对象不为 null.
    /// </summary>
    /// <typeparam name="T">对象类型.</typeparam>
    /// <param name="value">要检查的值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <returns>原始值.</returns>
    /// <exception cref="ArgumentNullException">值为 null 时抛出.</exception>
    public static T NotNull<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        return value;
    }

    /// <summary>
    /// 确保数值非负.
    /// </summary>
    /// <param name="value">要检查的值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <returns>原始值.</returns>
    /// <exception cref="ArgumentOutOfRangeException">值为负数时抛出.</exception>
    public static int NonNegative(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be non-negative.");
        }

        return value;
    }

    /// <summary>
    /// 确保数值大于零.
    /// </summary>
    /// <param name="value">要检查的值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <returns>原始值.</returns>
    /// <exception cref="ArgumentOutOfRangeException">值小于等于零时抛出.</exception>
    public static int Positive(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
        }

        return value;
    }
}

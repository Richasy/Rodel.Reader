// Copyright (c) Richasy. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Richasy.RodelReader.Utilities.DownloadKit.Helpers;

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
    /// <returns>非空参数值.</returns>
    /// <exception cref="ArgumentNullException">当参数为 null 时抛出.</exception>
    public static T NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        return value;
    }

    /// <summary>
    /// 确保字符串不为 null 或空白.
    /// </summary>
    /// <param name="value">字符串值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <returns>非空字符串.</returns>
    /// <exception cref="ArgumentException">当字符串为 null 或空白时抛出.</exception>
    public static string NotNullOrWhiteSpace([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        }

        return value;
    }

    /// <summary>
    /// 确保值在指定范围内.
    /// </summary>
    /// <typeparam name="T">值类型.</typeparam>
    /// <param name="value">要检查的值.</param>
    /// <param name="min">最小值（包含）.</param>
    /// <param name="max">最大值（包含）.</param>
    /// <param name="paramName">参数名称.</param>
    /// <returns>在范围内的值.</returns>
    /// <exception cref="ArgumentOutOfRangeException">当值不在范围内时抛出.</exception>
    public static T InRange<T>(T value, T min, T max, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max}.");
        }

        return value;
    }

    /// <summary>
    /// 确保值为正数.
    /// </summary>
    /// <param name="value">要检查的值.</param>
    /// <param name="paramName">参数名称.</param>
    /// <returns>正数值.</returns>
    /// <exception cref="ArgumentOutOfRangeException">当值不为正数时抛出.</exception>
    public static int Positive(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
        }

        return value;
    }
}

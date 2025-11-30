// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test;

/// <summary>
/// 断言扩展方法。
/// </summary>
internal static class AssertExtensions
{
    public static void ContainsText(string actual, string expected)
    {
        StringAssert.Contains(actual, expected, StringComparison.Ordinal);
    }

    public static void DoesNotContainText(string actual, string notExpected)
    {
        Assert.IsFalse(
            actual.Contains(notExpected, StringComparison.Ordinal),
            $"字符串不应包含 '{notExpected}'，但实际包含了。");
    }

    public static void StartsWithText(string actual, string expected)
    {
        StringAssert.StartsWith(actual, expected, StringComparison.Ordinal);
    }

    public static void IsNotNullOrEmpty(string? value, string message = "字符串不应为 null 或空")
    {
        Assert.IsFalse(string.IsNullOrEmpty(value), message);
    }

    public static void HasItems<T>(IReadOnlyList<T>? list, string message = "集合应包含至少一个元素")
    {
        Assert.IsNotNull(list, message);
        Assert.IsTrue(list.Count > 0, message);
    }
}

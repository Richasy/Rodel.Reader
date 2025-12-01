// Copyright (c) Reader Copilot. All rights reserved.

namespace FeedParser.Test;

/// <summary>
/// 断言扩展方法.
/// </summary>
internal static class AssertExtensions
{
    /// <summary>
    /// 断言字符串包含指定文本.
    /// </summary>
    public static void ContainsText(string actual, string expected)
    {
        StringAssert.Contains(actual, expected, StringComparison.Ordinal);
    }

    /// <summary>
    /// 断言字符串不包含指定文本.
    /// </summary>
    public static void DoesNotContainText(string actual, string notExpected)
    {
        Assert.IsFalse(
            actual.Contains(notExpected, StringComparison.Ordinal),
            $"String should not contain '{notExpected}' but it does.");
    }

    /// <summary>
    /// 断言 URI 有效.
    /// </summary>
    public static void IsValidUri(Uri? uri, string? message = null)
    {
        Assert.IsNotNull(uri, message ?? "URI should not be null");
        Assert.IsTrue(uri.IsAbsoluteUri, message ?? "URI should be absolute");
    }

    /// <summary>
    /// 断言日期时间在合理范围内.
    /// </summary>
    public static void IsReasonableDateTime(DateTimeOffset? dateTime, string? message = null)
    {
        Assert.IsNotNull(dateTime, message ?? "DateTime should not be null");

        // 检查日期是否在 1990 年到未来 1 年之间
        var minDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var maxDate = DateTimeOffset.UtcNow.AddYears(1);

        Assert.IsTrue(
            dateTime.Value >= minDate && dateTime.Value <= maxDate,
            message ?? $"DateTime {dateTime.Value} is not in reasonable range ({minDate} - {maxDate})");
    }

    /// <summary>
    /// 断言集合不为空.
    /// </summary>
    public static void IsNotEmpty<T>(IReadOnlyCollection<T>? collection, string? message = null)
    {
        Assert.IsNotNull(collection, message ?? "Collection should not be null");
        Assert.IsTrue(collection.Count > 0, message ?? "Collection should not be empty");
    }
}

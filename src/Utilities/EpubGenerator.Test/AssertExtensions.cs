// Copyright (c) Reader Copilot. All rights reserved.

namespace EpubGenerator.Test;

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
            $"String should not contain '{notExpected}' but it does.");
    }

    public static void StartsWithText(string actual, string expected)
    {
        StringAssert.StartsWith(actual, expected, StringComparison.Ordinal);
    }
}

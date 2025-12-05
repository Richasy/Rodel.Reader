// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.RodelReader.Utilities.FeedParser.Helpers;

namespace FeedParser.Test.Helpers;

/// <summary>
/// DateTimeHelper 单元测试.
/// </summary>
[TestClass]
public sealed class DateTimeHelperTests
{
    #region TryParseDate 测试

    [TestMethod]
    [DataRow("2024-01-15T10:30:00Z", 2024, 1, 15, 10, 30, 0)]
    [DataRow("2024-01-15T10:30:00+08:00", 2024, 1, 15, 2, 30, 0)] // 转换为 UTC
    [DataRow("2024-12-25T00:00:00-05:00", 2024, 12, 25, 5, 0, 0)] // 转换为 UTC
    public void TryParseDate_RFC3339_ShouldParseCorrectly(
        string input, int year, int month, int day, int hour, int minute, int second)
    {
        // Act
        var result = DateTimeHelper.TryParseDate(input, out var dateTime);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(year, dateTime.UtcDateTime.Year);
        Assert.AreEqual(month, dateTime.UtcDateTime.Month);
        Assert.AreEqual(day, dateTime.UtcDateTime.Day);
        Assert.AreEqual(hour, dateTime.UtcDateTime.Hour);
        Assert.AreEqual(minute, dateTime.UtcDateTime.Minute);
        Assert.AreEqual(second, dateTime.UtcDateTime.Second);
    }

    [TestMethod]
    [DataRow("Mon, 15 Jan 2024 10:30:00 GMT", 2024, 1, 15, 10, 30)]
    [DataRow("Sat, 01 Jan 2024 00:00:00 +0000", 2024, 1, 1, 0, 0)]
    [DataRow("Sun, 31 Dec 2023 23:59:59 -0500", 2024, 1, 1, 4, 59)] // 转换为 UTC
    public void TryParseDate_RFC822_ShouldParseCorrectly(
        string input, int year, int month, int day, int hour, int minute)
    {
        // Act
        var result = DateTimeHelper.TryParseDate(input, out var dateTime);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(year, dateTime.UtcDateTime.Year);
        Assert.AreEqual(month, dateTime.UtcDateTime.Month);
        Assert.AreEqual(day, dateTime.UtcDateTime.Day);
        Assert.AreEqual(hour, dateTime.UtcDateTime.Hour);
        Assert.AreEqual(minute, dateTime.UtcDateTime.Minute);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow(null)]
    public void TryParseDate_NullOrEmpty_ShouldReturnFalse(string? input)
    {
        // Act
        var result = DateTimeHelper.TryParseDate(input, out _);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    [DataRow("invalid date")]
    [DataRow("not a date at all")]
    // 注意："12:34:56" 可以被 DateTimeOffset.TryParse 解析为当天的时间
    public void TryParseDate_InvalidFormat_ShouldReturnFalse(string input)
    {
        // Act
        var result = DateTimeHelper.TryParseDate(input, out _);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    [DataRow("2024/01/15")]  // 斜杠格式
    [DataRow("January 15, 2024")]  // 英文月份
    [DataRow("15 Jan 2024")]  // 日月年格式
    public void TryParseDate_FlexibleFormats_ShouldSucceed(string input)
    {
        // Act - 库应该宽容地解析各种常见日期格式
        var result = DateTimeHelper.TryParseDate(input, out var date);

        // Assert
        Assert.IsTrue(result, $"应能解析 '{input}'");
        Assert.AreEqual(2024, date.Year);
        Assert.AreEqual(1, date.Month);
        Assert.AreEqual(15, date.Day);
    }

    [TestMethod]
    public void TryParseDate_TimeOnly_ShouldParseAsToday()
    {
        // Arrange - 仅时间格式会被解析为当天的时间
        var input = "12:34:56";

        // Act
        var result = DateTimeHelper.TryParseDate(input, out var date);

        // Assert
        Assert.IsTrue(result, "应能解析仅时间格式");
        Assert.AreEqual(DateTime.Today.Year, date.Year);
        Assert.AreEqual(DateTime.Today.Month, date.Month);
        Assert.AreEqual(DateTime.Today.Day, date.Day);
        Assert.AreEqual(12, date.Hour);
        Assert.AreEqual(34, date.Minute);
        Assert.AreEqual(56, date.Second);
    }

    #endregion

    #region TryParseDuration 测试

    [TestMethod]
    [DataRow("01:23:45", 5025)]  // 1*3600 + 23*60 + 45 = 5025
    [DataRow("00:05:30", 330)]   // 5*60 + 30 = 330
    [DataRow("12:00:00", 43200)] // 12*3600 = 43200
    public void TryParseDuration_HHmmss_ShouldParseCorrectly(string input, int expectedSeconds)
    {
        // Act
        var result = DateTimeHelper.TryParseDuration(input, out var seconds);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(expectedSeconds, seconds);
    }

    [TestMethod]
    [DataRow("23:45", 1425)]  // 23*60 + 45 = 1425
    [DataRow("05:30", 330)]   // 5*60 + 30 = 330
    [DataRow("00:00", 0)]
    public void TryParseDuration_mmss_ShouldParseCorrectly(string input, int expectedSeconds)
    {
        // Act
        var result = DateTimeHelper.TryParseDuration(input, out var seconds);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(expectedSeconds, seconds);
    }

    [TestMethod]
    [DataRow("3600", 3600)]  // 1 小时
    [DataRow("90", 90)]      // 1 分 30 秒
    [DataRow("45", 45)]      // 45 秒
    [DataRow("7200", 7200)]  // 2 小时
    public void TryParseDuration_Seconds_ShouldParseCorrectly(string input, int expectedSeconds)
    {
        // Act
        var result = DateTimeHelper.TryParseDuration(input, out var seconds);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(expectedSeconds, seconds);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow(null)]
    public void TryParseDuration_NullOrEmpty_ShouldReturnFalse(string? input)
    {
        // Act
        var result = DateTimeHelper.TryParseDuration(input, out _);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    [DataRow("invalid")]
    [DataRow("1:2:3:4")]
    [DataRow("abc:def:ghi")]
    public void TryParseDuration_InvalidFormat_ShouldReturnFalse(string input)
    {
        // Act
        var result = DateTimeHelper.TryParseDuration(input, out _);

        // Assert
        Assert.IsFalse(result);
    }

    #endregion
}


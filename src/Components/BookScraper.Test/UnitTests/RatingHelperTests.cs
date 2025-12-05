// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.BookScraper.Internal;

namespace BookScraper.Test.UnitTests;

[TestClass]
public class RatingHelperTests
{
    [TestMethod]
    [DataRow(10, 10, 5)]
    [DataRow(8, 10, 4)]
    [DataRow(5, 10, 3)]
    [DataRow(2, 10, 1)]
    [DataRow(0, 10, 0)]
    [DataRow(100, 100, 5)]
    [DataRow(50, 100, 3)]
    public void Normalize_ReturnsCorrectRating(double score, double maxScore, int expected)
    {
        var result = RatingHelper.Normalize(score, maxScore);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void Normalize_WithNegativeMaxScore_ReturnsZero()
    {
        var result = RatingHelper.Normalize(5, -10);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void Normalize_WithZeroMaxScore_ReturnsZero()
    {
        var result = RatingHelper.Normalize(5, 0);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [DataRow(9.5, 5)]
    [DataRow(7.8, 4)]
    [DataRow(5.0, 3)]
    [DataRow(2.5, 1)]
    public void FromTenScale_ReturnsCorrectRating(double score, int expected)
    {
        var result = RatingHelper.FromTenScale(score);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow("9.5", 10, 5)]
    [DataRow("7.8", 10, 4)]
    [DataRow("5.0", 10, 3)]
    [DataRow("", 10, 0)]
    [DataRow(null, 10, 0)]
    [DataRow("  ", 10, 0)]
    [DataRow("invalid", 10, 0)]
    public void ParseAndNormalize_ReturnsCorrectRating(string? scoreText, double maxScore, int expected)
    {
        var result = RatingHelper.ParseAndNormalize(scoreText, maxScore);
        Assert.AreEqual(expected, result);
    }
}

// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.BookScraper.Internal;

/// <summary>
/// 评分转换辅助类.
/// </summary>
internal static class RatingHelper
{
    /// <summary>
    /// 将评分标准化为 1-5 分制.
    /// </summary>
    /// <param name="score">原始评分.</param>
    /// <param name="maxScore">原始评分的最大值.</param>
    /// <returns>标准化后的评分（1-5）.</returns>
    public static int Normalize(double score, double maxScore)
    {
        if (maxScore <= 0 || score <= 0)
        {
            return 0;
        }

        var normalized = score / maxScore * 5;
        return (int)Math.Round(Math.Clamp(normalized, 1, 5), MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 从 10 分制转换为 5 分制.
    /// </summary>
    /// <param name="score">10 分制评分.</param>
    /// <returns>5 分制评分.</returns>
    public static int FromTenScale(double score)
        => Normalize(score, 10);

    /// <summary>
    /// 从字符串解析评分并标准化.
    /// </summary>
    /// <param name="scoreText">评分文本.</param>
    /// <param name="maxScore">原始评分的最大值.</param>
    /// <returns>标准化后的评分（1-5），解析失败返回 0.</returns>
    public static int ParseAndNormalize(string? scoreText, double maxScore)
    {
        if (string.IsNullOrWhiteSpace(scoreText))
        {
            return 0;
        }

        if (double.TryParse(scoreText.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var score))
        {
            return Normalize(score, maxScore);
        }

        return 0;
    }
}

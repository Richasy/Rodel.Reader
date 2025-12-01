// Copyright (c) Richasy. All rights reserved.

namespace Fb2Parser.Test;

/// <summary>
/// 断言扩展方法。
/// </summary>
internal static class AssertExtensions
{
    /// <summary>
    /// 断言字符串不为空且不为 null。
    /// </summary>
    public static void IsNotNullOrEmpty(string? value, string message = "")
    {
        Assert.IsFalse(string.IsNullOrEmpty(value), message);
    }

    /// <summary>
    /// 断言集合不为空。
    /// </summary>
    public static void IsNotEmpty<T>(IReadOnlyCollection<T>? collection, string message = "")
    {
        Assert.IsNotNull(collection, message);
        Assert.IsTrue(collection.Count > 0, message);
    }

    /// <summary>
    /// 断言集合为空。
    /// </summary>
    public static void IsEmpty<T>(IReadOnlyCollection<T>? collection, string message = "")
    {
        Assert.IsNotNull(collection, message);
        Assert.AreEqual(0, collection.Count, message);
    }

    /// <summary>
    /// 断言值在指定范围内。
    /// </summary>
    public static void IsInRange(int value, int min, int max, string message = "")
    {
        Assert.IsTrue(value >= min && value <= max, $"{message} Value {value} is not in range [{min}, {max}]");
    }

    /// <summary>
    /// 断言字节数组是有效的图片数据（通过检查魔数）。
    /// </summary>
    public static void IsValidImageData(byte[] data, string message = "")
    {
        Assert.IsNotNull(data, message);
        Assert.IsTrue(data.Length > 0, message);

        // 检查常见图片格式的魔数
        var isJpeg = data.Length >= 2 && data[0] == 0xFF && data[1] == 0xD8;
        var isPng = data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47;
        var isGif = data.Length >= 6 && data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46;
        var isBmp = data.Length >= 2 && data[0] == 0x42 && data[1] == 0x4D;
        var isWebp = data.Length >= 12 && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46;

        Assert.IsTrue(isJpeg || isPng || isGif || isBmp || isWebp, $"{message} Data does not appear to be a valid image");
    }
}

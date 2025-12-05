// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.BookScraper.Internal;

/// <summary>
/// 编码处理辅助类.
/// </summary>
internal static class EncodingHelper
{
    private static Encoding? _gbkEncoding;

    /// <summary>
    /// 获取 GBK 编码.
    /// </summary>
    /// <returns>GBK 编码实例.</returns>
    public static Encoding GetGbkEncoding()
    {
        if (_gbkEncoding is null)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _gbkEncoding = Encoding.GetEncoding("gbk");
        }

        return _gbkEncoding;
    }

    /// <summary>
    /// 将字符串编码为 GBK URL 编码格式.
    /// </summary>
    /// <param name="text">要编码的文本.</param>
    /// <returns>GBK URL 编码后的文本.</returns>
    public static string EncodeAsGbkUrl(string text)
    {
        var encoding = GetGbkEncoding();
        var bytes = encoding.GetBytes(text);
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "%{0:X2}", b);
        }

        return sb.ToString();
    }
}

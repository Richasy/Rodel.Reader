// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// 表示 Mobi 书籍的封面。
/// </summary>
public sealed class MobiCover
{
    private readonly Func<Task<byte[]>> _contentLoader;

    /// <summary>
    /// 初始化 <see cref="MobiCover"/> 类的新实例。
    /// </summary>
    /// <param name="mediaType">封面图片的媒体类型。</param>
    /// <param name="contentLoader">加载封面内容的函数。</param>
    public MobiCover(string mediaType, Func<Task<byte[]>> contentLoader)
    {
        MediaType = mediaType;
        _contentLoader = contentLoader;
    }

    /// <summary>
    /// 获取封面图片的媒体类型（如 image/jpeg）。
    /// </summary>
    public string MediaType { get; }

    /// <summary>
    /// 读取封面图片数据。
    /// </summary>
    /// <returns>封面图片的字节数组。</returns>
    public byte[] ReadContent()
    {
        return ReadContentAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// 异步读取封面图片数据。
    /// </summary>
    /// <returns>封面图片的字节数组。</returns>
    public Task<byte[]> ReadContentAsync()
    {
        return _contentLoader();
    }
}

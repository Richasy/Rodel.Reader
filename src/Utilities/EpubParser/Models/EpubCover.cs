// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 表示 EPUB 书籍的封面。
/// </summary>
public sealed class EpubCover
{
    private readonly Func<Task<byte[]>> _contentLoader;

    /// <summary>
    /// 初始化 <see cref="EpubCover"/> 类的新实例。
    /// </summary>
    /// <param name="resource">封面图片资源。</param>
    /// <param name="contentLoader">加载封面内容的函数。</param>
    public EpubCover(EpubResource resource, Func<Task<byte[]>> contentLoader)
    {
        Resource = resource;
        _contentLoader = contentLoader;
    }

    /// <summary>
    /// 获取封面图片资源信息。
    /// </summary>
    public EpubResource Resource { get; }

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

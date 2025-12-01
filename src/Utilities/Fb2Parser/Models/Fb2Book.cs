// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.Fb2Parser;

/// <summary>
/// 表示已解析的 FB2 书籍。
/// </summary>
public sealed class Fb2Book : IDisposable
{
    private readonly Dictionary<string, Fb2Binary> _binaries;
    private bool _disposed;

    internal Fb2Book(
        string? filePath,
        Fb2Metadata metadata,
        Fb2Cover? cover,
        List<Fb2NavItem> navigation,
        List<Fb2Section> sections,
        List<Fb2Binary> binaries)
    {
        FilePath = filePath;
        Metadata = metadata;
        Cover = cover;
        Navigation = navigation;
        Sections = sections;
        Binaries = binaries;
        _binaries = binaries.ToDictionary(b => b.Id, b => b, StringComparer.OrdinalIgnoreCase);
        Images = binaries.Where(b => b.IsImage).ToList();
    }

    /// <summary>
    /// 获取 FB2 文件的路径，如果从流加载则为 null。
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// 获取书籍的元数据。
    /// </summary>
    public Fb2Metadata Metadata { get; }

    /// <summary>
    /// 获取书籍的封面，如果不可用则为 null。
    /// </summary>
    public Fb2Cover? Cover { get; }

    /// <summary>
    /// 获取导航项（目录）。
    /// </summary>
    public IReadOnlyList<Fb2NavItem> Navigation { get; }

    /// <summary>
    /// 获取书籍的所有章节。
    /// </summary>
    public IReadOnlyList<Fb2Section> Sections { get; }

    /// <summary>
    /// 获取书籍中的所有二进制资源。
    /// </summary>
    public IReadOnlyList<Fb2Binary> Binaries { get; }

    /// <summary>
    /// 获取书籍中的所有图片资源。
    /// </summary>
    public IReadOnlyList<Fb2Binary> Images { get; }

    /// <summary>
    /// 根据 ID 查找二进制资源。
    /// </summary>
    /// <param name="id">资源 ID（可以带或不带 # 前缀）。</param>
    /// <returns>找到的资源，如果未找到则为 null。</returns>
    public Fb2Binary? FindBinaryById(string id)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 移除可能的 # 前缀
        var cleanId = id.TrimStart('#');
        return _binaries.GetValueOrDefault(cleanId);
    }

    /// <summary>
    /// 读取二进制资源的内容。
    /// </summary>
    /// <param name="binary">要读取的资源。</param>
    /// <returns>资源的字节数组。</returns>
    public byte[] ReadBinaryContent(Fb2Binary binary)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return binary.GetBytes();
    }

    /// <summary>
    /// 异步读取二进制资源的内容。
    /// </summary>
    /// <param name="binary">要读取的资源。</param>
    /// <returns>资源的字节数组。</returns>
    public Task<byte[]> ReadBinaryContentAsync(Fb2Binary binary)
    {
        return Task.FromResult(ReadBinaryContent(binary));
    }

    /// <summary>
    /// 根据 ID 读取二进制资源的内容。
    /// </summary>
    /// <param name="id">资源 ID。</param>
    /// <returns>资源的字节数组。</returns>
    public byte[] ReadBinaryContent(string id)
    {
        var binary = FindBinaryById(id) ?? throw new Fb2ParseException($"未找到二进制资源: {id}");
        return ReadBinaryContent(binary);
    }

    /// <summary>
    /// 异步根据 ID 读取二进制资源的内容。
    /// </summary>
    /// <param name="id">资源 ID。</param>
    /// <returns>资源的字节数组。</returns>
    public Task<byte[]> ReadBinaryContentAsync(string id)
    {
        return Task.FromResult(ReadBinaryContent(id));
    }

    /// <summary>
    /// 获取所有章节的扁平列表（包括嵌套章节）。
    /// </summary>
    /// <returns>所有章节的列表。</returns>
    public IReadOnlyList<Fb2Section> GetAllSections()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var result = new List<Fb2Section>();
        FlattenSections(Sections, result);
        return result;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    private static void FlattenSections(IEnumerable<Fb2Section> sections, List<Fb2Section> result)
    {
        foreach (var section in sections)
        {
            result.Add(section);
            if (section.HasChildren)
            {
                FlattenSections(section.Children, result);
            }
        }
    }
}

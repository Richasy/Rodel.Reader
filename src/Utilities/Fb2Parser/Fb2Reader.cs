// Copyright (c) Richasy. All rights reserved.

using System.Text;
using System.Xml.Linq;

namespace Richasy.RodelReader.Utilities.Fb2Parser;

/// <summary>
/// FB2 文件解析的主入口。
/// </summary>
public static class Fb2Reader
{
    private static readonly XNamespace FbNs = "http://www.gribuser.ru/xml/fictionbook/2.0";

    /// <summary>
    /// 从文件路径解析 FB2 文件。
    /// </summary>
    /// <param name="filePath">FB2 文件路径。</param>
    /// <returns>解析后的 FB2 书籍。</returns>
    public static Fb2Book Read(string filePath)
    {
        return ReadAsync(filePath).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 异步从文件路径解析 FB2 文件。
    /// </summary>
    /// <param name="filePath">FB2 文件路径。</param>
    /// <returns>解析后的 FB2 书籍。</returns>
    public static async Task<Fb2Book> ReadAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("未找到 FB2 文件", filePath);
        }

        var content = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
        using var stream = new MemoryStream(content);
        return await ParseStreamAsync(stream, filePath).ConfigureAwait(false);
    }

    /// <summary>
    /// 从流解析 FB2 文件。
    /// </summary>
    /// <param name="stream">包含 FB2 文件的流。</param>
    /// <returns>解析后的 FB2 书籍。</returns>
    public static Fb2Book Read(Stream stream)
    {
        return ReadAsync(stream).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 异步从流解析 FB2 文件。
    /// </summary>
    /// <param name="stream">包含 FB2 文件的流。</param>
    /// <returns>解析后的 FB2 书籍。</returns>
    public static async Task<Fb2Book> ReadAsync(Stream stream)
    {
        return await ParseStreamAsync(stream, null).ConfigureAwait(false);
    }

    /// <summary>
    /// 从字符串解析 FB2 内容。
    /// </summary>
    /// <param name="content">FB2 XML 内容。</param>
    /// <returns>解析后的 FB2 书籍。</returns>
    public static Fb2Book ReadFromString(string content)
    {
        return ReadFromStringAsync(content).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 异步从字符串解析 FB2 内容。
    /// </summary>
    /// <param name="content">FB2 XML 内容。</param>
    /// <returns>解析后的 FB2 书籍。</returns>
    public static async Task<Fb2Book> ReadFromStringAsync(string content)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return await ParseStreamAsync(stream, null).ConfigureAwait(false);
    }

    /// <summary>
    /// 解析 FB2 流。
    /// </summary>
    private static async Task<Fb2Book> ParseStreamAsync(Stream stream, string? filePath)
    {
        XDocument document;

        try
        {
            // 检测编码并解析 XML
            document = await LoadXmlAsync(stream).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new Fb2ParseException("无法解析 FB2 文件：XML 格式无效", ex);
        }

        var root = document.Root;
        if (root == null)
        {
            throw new Fb2ParseException("FB2 文件没有根元素");
        }

        // 验证是否为 FB2 格式
        if (root.Name.LocalName != "FictionBook")
        {
            throw new Fb2ParseException($"无效的 FB2 文件：根元素应为 FictionBook，实际为 {root.Name.LocalName}");
        }

        // 解析 description
        var descriptionElement = root.Element(FbNs + "description")
            ?? root.Elements().FirstOrDefault(e => e.Name.LocalName == "description");

        var metadata = DescriptionParser.Parse(descriptionElement);

        // 解析 body
        var bodyElements = root.Elements(FbNs + "body").ToList();
        if (bodyElements.Count == 0)
        {
            bodyElements = root.Elements().Where(e => e.Name.LocalName == "body").ToList();
        }

        var sections = BodyParser.Parse(bodyElements);

        // 解析 binary
        var binaryElements = root.Elements(FbNs + "binary").ToList();
        if (binaryElements.Count == 0)
        {
            binaryElements = root.Elements().Where(e => e.Name.LocalName == "binary").ToList();
        }

        var binaries = BinaryParser.Parse(binaryElements);

        // 提取导航
        var navigation = NavigationExtractor.Extract(sections);

        // 创建封面
        Fb2Cover? cover = null;
        if (!string.IsNullOrEmpty(metadata.CoverpageImageId))
        {
            var coverBinary = binaries.FirstOrDefault(b =>
                b.Id.Equals(metadata.CoverpageImageId, StringComparison.OrdinalIgnoreCase));

            if (coverBinary != null)
            {
                cover = new Fb2Cover(
                    coverBinary.Id,
                    coverBinary.MediaType,
                    () => Task.FromResult(coverBinary.GetBytes()));
            }
        }

        // 如果没有通过 coverpage 找到封面，尝试找第一张图片
        if (cover == null && binaries.Count > 0)
        {
            var firstImage = binaries.FirstOrDefault(b => b.IsImage);
            if (firstImage != null)
            {
                cover = new Fb2Cover(
                    firstImage.Id,
                    firstImage.MediaType,
                    () => Task.FromResult(firstImage.GetBytes()));
            }
        }

        return new Fb2Book(
            filePath,
            metadata,
            cover,
            navigation,
            sections,
            binaries);
    }

    private static async Task<XDocument> LoadXmlAsync(Stream stream)
    {
        // 尝试检测编码
        var encoding = await DetectEncodingAsync(stream).ConfigureAwait(false);
        stream.Position = 0;

        using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var content = await reader.ReadToEndAsync().ConfigureAwait(false);

        return XDocument.Parse(content);
    }

    private static async Task<Encoding> DetectEncodingAsync(Stream stream)
    {
        // 读取足够的字节来检测 BOM 或 XML 声明
        var buffer = new byte[1024];
        var bytesRead = await stream.ReadAsync(buffer).ConfigureAwait(false);

        if (bytesRead >= 3)
        {
            // 检测 BOM
            if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                return Encoding.UTF8;
            }

            if (buffer[0] == 0xFF && buffer[1] == 0xFE)
            {
                return Encoding.Unicode;
            }

            if (buffer[0] == 0xFE && buffer[1] == 0xFF)
            {
                return Encoding.BigEndianUnicode;
            }
        }

        // 尝试从 XML 声明中提取编码
        var asciiPreview = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        var encodingMatch = System.Text.RegularExpressions.Regex.Match(
            asciiPreview,
            @"encoding\s*=\s*[""']([^""']+)[""']",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (encodingMatch.Success)
        {
            var encodingName = encodingMatch.Groups[1].Value;
            try
            {
                return Encoding.GetEncoding(encodingName);
            }
            catch
            {
                // 忽略无效编码，回退到 UTF-8
            }
        }

        // 默认使用 UTF-8
        return Encoding.UTF8;
    }
}

// Copyright (c) Richasy. All rights reserved.

using System.IO.Compression;
using System.Xml.Linq;

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 读取 container.xml 以查找根文件路径。
/// </summary>
internal static class ContainerReader
{
    private const string ContainerPath = "META-INF/container.xml";

    /// <summary>
    /// 从 container.xml 获取 OPF 文件的路径。
    /// </summary>
    public static async Task<string> GetRootFilePathAsync(ZipArchive archive)
    {
        var entry = archive.GetEntry(ContainerPath)
            ?? archive.Entries.FirstOrDefault(e =>
                e.FullName.Equals(ContainerPath, StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            throw new EpubParseException($"未找到容器文件：{ContainerPath}");
        }

        using var stream = entry.Open();
        var document = await XmlHelper.LoadDocumentAsync(stream).ConfigureAwait(false);

        if (document == null)
        {
            throw new EpubParseException("解析 container.xml 失败");
        }

        // 尝试查找 rootfile 元素
        var rootfile = FindRootFileElement(document.Root);

        if (rootfile == null)
        {
            throw new EpubParseException("在 container.xml 中未找到根文件路径");
        }

        var fullPath = rootfile.GetAttributeValue("full-path");

        if (string.IsNullOrEmpty(fullPath))
        {
            throw new EpubParseException("根文件路径属性为空");
        }

        return fullPath;
    }

    private static XElement? FindRootFileElement(XElement? root)
    {
        if (root == null)
        {
            return null;
        }

        // 尝试标准路径
        var rootfiles = root.GetElement("rootfiles");
        if (rootfiles != null)
        {
            var rootfile = rootfiles.GetElement("rootfile");
            if (rootfile != null)
            {
                return rootfile;
            }
        }

        // 回退：搜索后代元素
        foreach (var element in root.Descendants())
        {
            if (element.HasLocalName("rootfile"))
            {
                return element;
            }
        }

        return null;
    }
}

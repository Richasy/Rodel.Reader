// Copyright (c) Richasy. All rights reserved.

using System.IO.Compression;
using System.Xml.Linq;

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 读取并解析 OPF 包文件。
/// </summary>
internal static class PackageReader
{
    /// <summary>
    /// 解析后的 OPF 包数据。
    /// </summary>
    public sealed class PackageData
    {
        /// <summary>
        /// 获取或设置元数据。
        /// </summary>
        public EpubMetadata Metadata { get; set; } = new();

        /// <summary>
        /// 获取或设置资源列表。
        /// </summary>
        public List<EpubResource> Resources { get; set; } = [];

        /// <summary>
        /// 获取或设置阅读顺序。
        /// </summary>
        public List<EpubResource> ReadingOrder { get; set; } = [];

        /// <summary>
        /// 获取或设置封面资源。
        /// </summary>
        public EpubResource? CoverResource { get; set; }

        /// <summary>
        /// 获取或设置元数据元素。
        /// </summary>
        public XElement? MetadataElement { get; set; }

        /// <summary>
        /// 获取或设置 spine 元素。
        /// </summary>
        public XElement? SpineElement { get; set; }
    }

    /// <summary>
    /// 解析 OPF 包文件。
    /// </summary>
    public static async Task<PackageData> ParseAsync(
        ZipArchive archive,
        string rootFilePath,
        string contentDirectoryPath)
    {
        var entry = archive.GetEntry(rootFilePath)
            ?? archive.Entries.FirstOrDefault(e =>
                e.FullName.Equals(rootFilePath, StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            throw new EpubParseException($"未找到 OPF 文件：{rootFilePath}");
        }

        using var stream = entry.Open();
        var document = await XmlHelper.LoadDocumentAsync(stream).ConfigureAwait(false);

        if (document?.Root == null)
        {
            throw new EpubParseException("解析 OPF 包失败");
        }

        var package = document.Root;
        var data = new PackageData();

        // 解析元数据
        var metadataElement = package.GetElement("metadata");
        data.MetadataElement = metadataElement;
        data.Metadata = MetadataParser.Parse(metadataElement);

        // 解析 manifest
        var manifestElement = package.GetElement("manifest");
        data.Resources = ManifestParser.Parse(manifestElement, contentDirectoryPath);

        // 解析 spine
        var spineElement = package.GetElement("spine");
        data.SpineElement = spineElement;
        data.ReadingOrder = SpineParser.Parse(spineElement, data.Resources);

        // 查找封面
        var guideElement = package.GetElement("guide");
        data.CoverResource = CoverFinder.Find(data.Resources, metadataElement, guideElement);

        return data;
    }
}

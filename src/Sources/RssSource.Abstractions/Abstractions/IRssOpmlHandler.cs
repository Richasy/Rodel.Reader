// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS OPML 处理接口.
/// 定义 OPML 导入导出能力.
/// </summary>
public interface IRssOpmlHandler
{
    /// <summary>
    /// 导入 OPML 文件.
    /// </summary>
    /// <param name="opmlContent">OPML 文件内容.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否导入成功.</returns>
    /// <remarks>
    /// 并非所有 RSS 源都支持 OPML 导入，调用前应检查 <see cref="IRssSourceCapabilities.CanImportOpml"/>.
    /// </remarks>
    Task<bool> ImportOpmlAsync(
        string opmlContent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 导出为 OPML 格式.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>OPML 文件内容.</returns>
    /// <remarks>
    /// 所有 RSS 源都应支持 OPML 导出（本地生成）.
    /// </remarks>
    Task<string> ExportOpmlAsync(CancellationToken cancellationToken = default);
}

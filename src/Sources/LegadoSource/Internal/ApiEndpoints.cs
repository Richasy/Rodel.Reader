// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.Legado.Models.Enums;

namespace Richasy.RodelReader.Sources.Legado.Internal;

/// <summary>
/// API 端点常量.
/// </summary>
internal static class ApiEndpoints
{
    // 书架相关
    public const string GetBookshelf = "getBookshelf";
    public const string SaveBook = "saveBook";
    public const string DeleteBook = "deleteBook";

    // 章节相关
    public const string GetChapterList = "getChapterList";
    public const string GetBookContent = "getBookContent";

    // 进度相关
    public const string SaveBookProgress = "saveBookProgress";

    // 书源相关
    public const string GetBookSources = "getBookSources";
    public const string GetBookSource = "getBookSource";
    public const string SaveBookSource = "saveBookSource";
    public const string SaveBookSources = "saveBookSources";
    public const string DeleteBookSources = "deleteBookSources";

    // 封面
    public const string Cover = "cover";

    /// <summary>
    /// 获取完整的 API URL.
    /// </summary>
    /// <param name="baseUrl">基础 URL.</param>
    /// <param name="endpoint">端点名称.</param>
    /// <param name="serverType">服务器类型.</param>
    /// <param name="accessToken">访问令牌（可选）.</param>
    /// <param name="queryParams">查询参数（可选）.</param>
    /// <returns>完整的 API URL.</returns>
    public static string BuildUrl(
        string baseUrl,
        string endpoint,
        ServerType serverType,
        string? accessToken = null,
        Dictionary<string, string>? queryParams = null)
    {
        var normalizedBase = baseUrl.TrimEnd('/');

        // hectorqin/reader 使用 /reader3/ 前缀
        var path = serverType == ServerType.HectorqinReader
            ? $"{normalizedBase}/reader3/{endpoint}"
            : $"{normalizedBase}/{endpoint}";

        // 构建查询字符串
        var queryParts = new List<string>();

        if (!string.IsNullOrEmpty(accessToken))
        {
            queryParts.Add($"accessToken={Uri.EscapeDataString(accessToken)}");
        }

        if (queryParams != null)
        {
            foreach (var param in queryParams)
            {
                queryParts.Add($"{param.Key}={Uri.EscapeDataString(param.Value)}");
            }
        }

        if (queryParts.Count > 0)
        {
            path += "?" + string.Join("&", queryParts);
        }

        return path;
    }
}

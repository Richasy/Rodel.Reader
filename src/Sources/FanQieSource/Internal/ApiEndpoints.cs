// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Internal;

/// <summary>
/// API 端点常量.
/// </summary>
internal static class ApiEndpoints
{
    /// <summary>
    /// 搜索 API 基础 URL.
    /// </summary>
    public const string SearchBase = "https://api-lf.fanqiesdk.com/api/novel/channel/homepage/search/search/v1/";

    /// <summary>
    /// 书籍详情 API 基础 URL.
    /// </summary>
    public const string BookDetailBase = "https://api5-normal-sinfonlineb.fqnovel.com/reading/bookapi/multi-detail/v/";

    /// <summary>
    /// 书籍目录 API URL.
    /// </summary>
    public const string BookToc = "https://fanqienovel.com/api/reader/directory/detail";

    /// <summary>
    /// 批量内容 API 基础 URL.
    /// </summary>
    public const string BatchContentBase = "https://api5-normal-sinfonlineb.fqnovel.com/reading/reader/batch_full/v";

    /// <summary>
    /// 注册密钥 API URL.
    /// </summary>
    public const string RegisterKey = "https://api5-normal-sinfonlineb.fqnovel.com/reading/crypt/registerkey";

    /// <summary>
    /// 外部设备注册 API URL（第三方服务）.
    /// </summary>
    public const string ExternalDeviceRegister = "https://fq.shusan.cn/api/device/register";

    /// <summary>
    /// 外部内容获取 API URL（第三方服务）.
    /// </summary>
    public const string ExternalContent = "https://fq.shusan.cn/api/content";

    /// <summary>
    /// 获取搜索 URL.
    /// </summary>
    /// <param name="query">搜索关键词.</param>
    /// <param name="offset">偏移量.</param>
    /// <param name="aid">应用 ID.</param>
    /// <returns>搜索 URL.</returns>
    public static string GetSearchUrl(string query, int offset, string aid)
    {
        var encodedQuery = Uri.EscapeDataString(query);
        return $"{SearchBase}?offset={offset}&aid={aid}&q={encodedQuery}";
    }

    /// <summary>
    /// 获取书籍详情 URL.
    /// </summary>
    /// <param name="bookIds">书籍 ID 列表（逗号分隔）.</param>
    /// <param name="aid">应用 ID.</param>
    /// <returns>书籍详情 URL.</returns>
    public static string GetBookDetailUrl(string bookIds, string aid)
    {
        return $"{BookDetailBase}?book_id={bookIds}&aid={aid}";
    }

    /// <summary>
    /// 获取书籍目录 URL.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <returns>书籍目录 URL.</returns>
    public static string GetBookTocUrl(string bookId)
    {
        return $"{BookToc}?bookId={bookId}";
    }

    /// <summary>
    /// 获取批量内容 URL.
    /// </summary>
    /// <param name="itemIds">章节 ID 列表（逗号分隔）.</param>
    /// <param name="aid">应用 ID.</param>
    /// <param name="updateVersionCode">更新版本码.</param>
    /// <returns>批量内容 URL.</returns>
    public static string GetBatchContentUrl(string itemIds, string aid, string updateVersionCode)
    {
        return $"{BatchContentBase}?item_ids={itemIds}&req_type=1&aid={aid}&update_version_code={updateVersionCode}";
    }
}

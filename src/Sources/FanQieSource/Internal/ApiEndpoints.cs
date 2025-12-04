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

    #region 后备 API

    /// <summary>
    /// 获取后备搜索 URL.
    /// </summary>
    /// <param name="baseUrl">后备 API 基础 URL.</param>
    /// <param name="query">搜索关键词.</param>
    /// <param name="offset">偏移量.</param>
    /// <param name="count">每页数量.</param>
    /// <returns>后备搜索 URL.</returns>
    public static string GetFallbackSearchUrl(string baseUrl, string query, int offset, int count = 20)
    {
        var encodedQuery = Uri.EscapeDataString(query);
        return $"{baseUrl}/api/fqsearch/books?query={encodedQuery}&offset={offset}&count={count}";
    }

    /// <summary>
    /// 获取后备书籍详情 URL.
    /// </summary>
    /// <param name="baseUrl">后备 API 基础 URL.</param>
    /// <param name="bookId">书籍 ID.</param>
    /// <returns>后备书籍详情 URL.</returns>
    public static string GetFallbackBookDetailUrl(string baseUrl, string bookId)
    {
        return $"{baseUrl}/api/fqnovel/book/{bookId}";
    }

    /// <summary>
    /// 获取后备书籍目录 URL.
    /// </summary>
    /// <param name="baseUrl">后备 API 基础 URL.</param>
    /// <param name="bookId">书籍 ID.</param>
    /// <returns>后备书籍目录 URL.</returns>
    public static string GetFallbackBookTocUrl(string baseUrl, string bookId)
    {
        return $"{baseUrl}/api/fqsearch/directory/{bookId}";
    }

    /// <summary>
    /// 获取后备批量章节内容 URL.
    /// </summary>
    /// <param name="baseUrl">后备 API 基础 URL.</param>
    /// <returns>后备批量章节内容 URL.</returns>
    public static string GetFallbackBatchContentUrl(string baseUrl)
    {
        return $"{baseUrl}/api/fqnovel/chapters/batch";
    }

    #endregion

    #region 段评 API

    /// <summary>
    /// 段评数量 API 基础 URL.
    /// </summary>
    public const string CommentCountBase = "https://api5-normal-sinfonlinec.fqnovel.com/reading/ugc/idea/list/v/";

    /// <summary>
    /// 段评列表 API 基础 URL.
    /// </summary>
    public const string CommentListBase = "https://api5-normal-sinfonlinec.fqnovel.com/reading/ugc/idea/comment_list/v/";

    /// <summary>
    /// 获取段评数量 URL.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="chapterId">章节 ID.</param>
    /// <param name="aid">应用 ID.</param>
    /// <returns>段评数量 URL.</returns>
    public static string GetCommentCountUrl(string bookId, string chapterId, string aid)
    {
        return $"{CommentCountBase}?item_version=3add812e2984c508c71ce1361c31cf5f_1_v5&book_id={bookId}&aid={aid}&version_code=513&item_id={chapterId}";
    }

    /// <summary>
    /// 获取段评列表 URL.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="chapterId">章节 ID.</param>
    /// <param name="paragraphIndex">段落索引.</param>
    /// <param name="aid">应用 ID.</param>
    /// <param name="offset">分页偏移量.</param>
    /// <returns>段评列表 URL.</returns>
    public static string GetCommentListUrl(string bookId, string chapterId, int paragraphIndex, string aid, string? offset = null)
    {
        var url = $"{CommentListBase}?item_version=3add812e2984c508c71ce1361c31cf5f_1_v5&book_id={bookId}&aid={aid}&version_code=513&item_id={chapterId}&para_index={paragraphIndex}";
        if (!string.IsNullOrEmpty(offset))
        {
            url += $"&offset={offset}";
        }

        return url;
    }

    #endregion
}

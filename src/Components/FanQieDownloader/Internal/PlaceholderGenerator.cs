// Copyright (c) Richasy. All rights reserved.

using System.Web;

namespace Richasy.RodelReader.Components.FanQie.Internal;

/// <summary>
/// 占位内容生成器，用于生成失败章节的占位 HTML.
/// </summary>
/// <remarks>
/// 注意：所有生成方法只返回 body 内部的内容片段，不包含完整的 HTML 结构。
/// EpubBuilder 会负责将内容包装到完整的 XHTML 结构中。
/// 元数据通过 HTML 注释和 data-* 属性保存，以便同步时识别章节状态。
/// </remarks>
internal static class PlaceholderGenerator
{
    /// <summary>
    /// 生成失败章节的占位 HTML 内容.
    /// </summary>
    /// <param name="chapterId">章节 ID.</param>
    /// <param name="chapterTitle">章节标题.</param>
    /// <param name="order">章节序号.</param>
    /// <param name="reason">失败原因.</param>
    /// <returns>占位 HTML 内容片段（仅 body 内部内容）.</returns>
    public static string GenerateFailedPlaceholder(
        string chapterId,
        string chapterTitle,
        int order,
        string? reason = null)
    {
        var encodedReason = HttpUtility.HtmlEncode(reason ?? "网络原因");

        // 使用 HTML 注释存储元数据，使用 data-* 属性标记状态
        return $"""
            <!-- fanqie:chapter-id={chapterId} -->
            <!-- fanqie:chapter-order={order} -->
            <!-- fanqie:status=failed -->
            <!-- fanqie:fail-reason={encodedReason} -->
            <div class="chapter-unavailable" data-fanqie-chapter-id="{chapterId}" data-fanqie-status="failed">
                <div class="error-content">
                    <p class="error-message">由于{encodedReason}，本章节内容暂时无法下载。</p>
                    <p class="retry-hint">下次同步时将自动重试。</p>
                </div>
            </div>
            """;
    }

    /// <summary>
    /// 生成锁定章节的占位 HTML 内容.
    /// </summary>
    /// <param name="chapterId">章节 ID.</param>
    /// <param name="chapterTitle">章节标题.</param>
    /// <param name="order">章节序号.</param>
    /// <returns>占位 HTML 内容片段（仅 body 内部内容）.</returns>
    public static string GenerateLockedPlaceholder(
        string chapterId,
        string chapterTitle,
        int order)
    {
        // 使用 HTML 注释存储元数据，使用 data-* 属性标记状态
        return $"""
            <!-- fanqie:chapter-id={chapterId} -->
            <!-- fanqie:chapter-order={order} -->
            <!-- fanqie:status=locked -->
            <div class="chapter-locked" data-fanqie-chapter-id="{chapterId}" data-fanqie-status="locked">
                <div class="locked-content">
                    <p class="locked-message">本章节需要付费阅读，暂不支持下载。</p>
                </div>
            </div>
            """;
    }

    /// <summary>
    /// 包装正常章节的内容，添加元数据标记.
    /// </summary>
    /// <param name="chapterId">章节 ID.</param>
    /// <param name="chapterTitle">章节标题.</param>
    /// <param name="order">章节序号.</param>
    /// <param name="bodyContent">正文内容（已添加段落标记）.</param>
    /// <returns>带元数据标记的 HTML 内容片段（仅 body 内部内容）.</returns>
    public static string WrapChapterContent(
        string chapterId,
        string chapterTitle,
        int order,
        string bodyContent)
    {
        // 使用 HTML 注释存储元数据
        return $"""
            <!-- fanqie:chapter-id={chapterId} -->
            <!-- fanqie:chapter-order={order} -->
            <!-- fanqie:status=downloaded -->
            {bodyContent}
            """;
    }
}

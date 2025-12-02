// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.FanQie.Models.Internal;

/// <summary>
/// 书籍目录 API 响应.
/// </summary>
internal sealed class BookTocApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public BookTocData? Data { get; set; }
}

internal sealed class BookTocData
{
    [JsonPropertyName("allItemIds")]
    public IReadOnlyList<string>? AllItemIds { get; set; }

    [JsonPropertyName("volumeNameList")]
    public IReadOnlyList<string>? VolumeNameList { get; set; }

    [JsonPropertyName("chapterListWithVolume")]
    public IReadOnlyList<IReadOnlyList<TocChapterItem>>? ChapterListWithVolume { get; set; }
}

internal sealed class TocChapterItem
{
    [JsonPropertyName("itemId")]
    public string? ItemId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("needPay")]
    public int NeedPay { get; set; }

    [JsonPropertyName("isChapterLock")]
    public bool IsChapterLock { get; set; }

    [JsonPropertyName("volume_name")]
    public string? VolumeName { get; set; }

    [JsonPropertyName("realChapterOrder")]
    public string? RealChapterOrder { get; set; }

    [JsonPropertyName("firstPassTime")]
    public string? FirstPassTime { get; set; }
}

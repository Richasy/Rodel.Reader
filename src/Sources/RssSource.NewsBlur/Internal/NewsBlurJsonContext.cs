// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.NewsBlur.Internal;

/// <summary>
/// AOT 兼容的 JSON 序列化上下文.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(NewsBlurAuthResult))]
[JsonSerializable(typeof(NewsBlurFeed))]
[JsonSerializable(typeof(NewsBlurStory))]
[JsonSerializable(typeof(NewsBlurFeedsResponse))]
[JsonSerializable(typeof(NewsBlurStoriesResponse))]
[JsonSerializable(typeof(NewsBlurAddFeedResponse))]
[JsonSerializable(typeof(NewsBlurOperationResponse))]
[JsonSerializable(typeof(Dictionary<string, NewsBlurFeed>))]
[JsonSerializable(typeof(List<NewsBlurStory>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, List<string>>))]
internal sealed partial class NewsBlurJsonContext : JsonSerializerContext
{
}

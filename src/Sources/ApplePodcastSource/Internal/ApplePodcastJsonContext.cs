// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ApplePodcast.Internal;

/// <summary>
/// AOT 兼容的 JSON 序列化上下文.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ITunesCategoryResponse))]
[JsonSerializable(typeof(ITunesSearchResponse))]
internal sealed partial class ApplePodcastJsonContext : JsonSerializerContext
{
}

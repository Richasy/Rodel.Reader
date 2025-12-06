// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Miniflux.Internal;

/// <summary>
/// AOT 兼容的 JSON 序列化上下文.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(MinifluxUser))]
[JsonSerializable(typeof(MinifluxCategory))]
[JsonSerializable(typeof(List<MinifluxCategory>))]
[JsonSerializable(typeof(MinifluxFeed))]
[JsonSerializable(typeof(List<MinifluxFeed>))]
[JsonSerializable(typeof(MinifluxEntry))]
[JsonSerializable(typeof(List<MinifluxEntry>))]
[JsonSerializable(typeof(MinifluxEntriesResponse))]
[JsonSerializable(typeof(MinifluxEnclosure))]
[JsonSerializable(typeof(List<MinifluxEnclosure>))]
[JsonSerializable(typeof(MinifluxIcon))]
[JsonSerializable(typeof(MinifluxIconData))]
[JsonSerializable(typeof(MinifluxCreateFeedRequest))]
[JsonSerializable(typeof(MinifluxCreateFeedResponse))]
[JsonSerializable(typeof(MinifluxUpdateFeedRequest))]
[JsonSerializable(typeof(MinifluxCreateCategoryRequest))]
[JsonSerializable(typeof(MinifluxUpdateCategoryRequest))]
[JsonSerializable(typeof(MinifluxUpdateEntriesRequest))]
[JsonSerializable(typeof(MinifluxDiscoverRequest))]
[JsonSerializable(typeof(MinifluxDiscoveredFeed))]
[JsonSerializable(typeof(List<MinifluxDiscoveredFeed>))]
[JsonSerializable(typeof(MinifluxErrorResponse))]
[JsonSerializable(typeof(MinifluxImportResponse))]
internal sealed partial class MinifluxJsonContext : JsonSerializerContext
{
}

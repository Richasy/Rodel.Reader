// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Feedbin.Internal;

/// <summary>
/// AOT 兼容的 JSON 序列化上下文.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(List<FeedbinSubscription>))]
[JsonSerializable(typeof(FeedbinSubscription))]
[JsonSerializable(typeof(List<FeedbinEntry>))]
[JsonSerializable(typeof(FeedbinEntry))]
[JsonSerializable(typeof(List<FeedbinTagging>))]
[JsonSerializable(typeof(FeedbinTagging))]
[JsonSerializable(typeof(List<long>))]
[JsonSerializable(typeof(FeedbinCreateSubscriptionRequest))]
[JsonSerializable(typeof(FeedbinUpdateSubscriptionRequest))]
[JsonSerializable(typeof(FeedbinCreateTaggingRequest))]
[JsonSerializable(typeof(FeedbinUnreadEntriesRequest))]
[JsonSerializable(typeof(FeedbinImportResponse))]
[JsonSerializable(typeof(List<FeedbinImportResponse>))]
[JsonSerializable(typeof(List<FeedbinDiscoveredFeed>))]
internal sealed partial class FeedbinJsonContext : JsonSerializerContext
{
}

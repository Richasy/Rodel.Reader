// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.GoogleReader.Internal;

/// <summary>
/// AOT 兼容的 JSON 序列化上下文.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(GoogleReaderAuthResponse))]
[JsonSerializable(typeof(GoogleReaderSubscriptionResponse))]
[JsonSerializable(typeof(GoogleReaderTagListResponse))]
[JsonSerializable(typeof(GoogleReaderStreamContentResponse))]
internal sealed partial class GoogleReaderJsonContext : JsonSerializerContext
{
}

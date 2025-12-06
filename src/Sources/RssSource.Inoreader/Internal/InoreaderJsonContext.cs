// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Inoreader.Internal;

/// <summary>
/// AOT 兼容的 JSON 序列化上下文.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(InoreaderAuthResponse))]
[JsonSerializable(typeof(InoreaderSubscriptionResponse))]
[JsonSerializable(typeof(InoreaderTagListResponse))]
[JsonSerializable(typeof(InoreaderPreferenceResponse))]
[JsonSerializable(typeof(InoreaderStreamContentResponse))]
internal sealed partial class InoreaderJsonContext : JsonSerializerContext
{
}

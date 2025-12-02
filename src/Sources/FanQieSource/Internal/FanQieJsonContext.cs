// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.FanQie.Internal;

/// <summary>
/// JSON 序列化上下文（AOT 兼容）.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
[JsonSerializable(typeof(Models.Internal.SearchApiResponse))]
[JsonSerializable(typeof(Models.Internal.BookDetailApiResponse))]
[JsonSerializable(typeof(Models.Internal.BookTocApiResponse))]
[JsonSerializable(typeof(Models.Internal.BatchContentApiResponse))]
[JsonSerializable(typeof(Models.Internal.CryptKeyApiResponse))]
[JsonSerializable(typeof(Models.Internal.RegisterKeyRequest))]
[JsonSerializable(typeof(Models.Internal.DeviceRegisterApiResponse))]
[JsonSerializable(typeof(Models.Internal.ExternalContentApiResponse))]
[JsonSerializable(typeof(Models.Internal.ExternalBatchContentApiResponse))]
[JsonSerializable(typeof(List<Models.Internal.CategoryV2Item>))]
internal sealed partial class FanQieJsonContext : JsonSerializerContext
{
}

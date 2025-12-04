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
[JsonSerializable(typeof(List<Models.Internal.CategoryV2Item>))]
[JsonSerializable(typeof(Models.Internal.FallbackSearchApiResponse))]
[JsonSerializable(typeof(Models.Internal.FallbackBookDetailApiResponse))]
[JsonSerializable(typeof(Models.Internal.FallbackBookTocApiResponse))]
[JsonSerializable(typeof(Models.Internal.FallbackBatchContentApiResponse))]
[JsonSerializable(typeof(Models.Internal.FallbackBatchContentRequest))]
[JsonSerializable(typeof(Models.Internal.DeviceRegisterApiResponse))]
[JsonSerializable(typeof(Models.Internal.DeviceReleaseResponse))]
[JsonSerializable(typeof(Models.Internal.DataResponse<Models.Internal.CommentCountData>))]
[JsonSerializable(typeof(Models.Internal.DataResponse<Models.Internal.CommentListData>))]
internal sealed partial class FanQieJsonContext : JsonSerializerContext
{
}

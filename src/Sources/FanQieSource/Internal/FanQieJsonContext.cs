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
[JsonSerializable(typeof(Models.Internal.DataResponse<Models.Internal.CommentCountData>))]
[JsonSerializable(typeof(Models.Internal.DataResponse<Models.Internal.CommentListData>))]
[JsonSerializable(typeof(Models.Internal.ExternalRemoteConfig))]
[JsonSerializable(typeof(Models.Internal.ExternalSearchApiResponse))]
[JsonSerializable(typeof(Models.Internal.ExternalBookDetailApiResponse))]
[JsonSerializable(typeof(Models.Internal.ExternalBookTocApiResponse))]
[JsonSerializable(typeof(Models.Internal.ExternalChapterContentApiResponse))]
[JsonSerializable(typeof(Models.Internal.ExternalBatchContentApiResponse))]
[JsonSerializable(typeof(Models.Internal.ExternalFullBookApiResponse))]
internal sealed partial class FanQieJsonContext : JsonSerializerContext
{
}

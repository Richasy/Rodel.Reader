// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models.Internal;

/// <summary>
/// ZLibrary JSON 序列化上下文（用于 AOT）.
/// </summary>
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(LoginResponseData))]
[JsonSerializable(typeof(BooklistApiResponse))]
[JsonSerializable(typeof(BooklistBookWrapper))]
[JsonSerializable(typeof(BooklistBookData))]
[JsonSerializable(typeof(BooklistPagination))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ZLibraryJsonContext : JsonSerializerContext
{
}

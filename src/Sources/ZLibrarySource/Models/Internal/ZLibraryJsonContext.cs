// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models.Internal;

/// <summary>
/// ZLibrary JSON 序列化上下文（用于 AOT）.
/// </summary>
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(LoginResponseData))]
[JsonSerializable(typeof(ProfileApiResponse))]
[JsonSerializable(typeof(ProfileApiUser))]
[JsonSerializable(typeof(SearchApiResponse))]
[JsonSerializable(typeof(SearchApiPagination))]
[JsonSerializable(typeof(SearchApiBook))]
[JsonSerializable(typeof(DownloadApiResponse))]
[JsonSerializable(typeof(DownloadApiFile))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ZLibraryJsonContext : JsonSerializerContext
{
}

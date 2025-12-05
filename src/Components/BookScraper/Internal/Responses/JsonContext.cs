// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.BookScraper.Internal.Responses.DouBan;
using Richasy.RodelReader.Components.BookScraper.Internal.Responses.Google;
using Richasy.RodelReader.Components.BookScraper.Internal.Responses.Pixiv;

namespace Richasy.RodelReader.Components.BookScraper.Internal.Responses;

/// <summary>
/// JSON 序列化上下文.
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(DouBanSearchResponse))]
[JsonSerializable(typeof(DouBanBook))]
[JsonSerializable(typeof(GoogleSearchResponse))]
[JsonSerializable(typeof(GoogleBookItem))]
[JsonSerializable(typeof(PixivSearchResponse))]
internal sealed partial class JsonContext : JsonSerializerContext
{
}

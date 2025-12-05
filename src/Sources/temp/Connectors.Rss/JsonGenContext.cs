// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.ReaderKernel.Connectors.Rss.Models.Feedbin;
using Richasy.ReaderKernel.Connectors.Rss.Models.NewsBlur;
using Richasy.ReaderKernel.Models.Config;
using System.Text.Json.Serialization;

namespace Richasy.ReaderKernel.Connectors.Rss;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(FeedbinMarkReadRequest))]
[JsonSerializable(typeof(FeedbinUpdateGroupRequest))]
[JsonSerializable(typeof(List<long>))]
[JsonSerializable(typeof(FeedbinSubscription))]
[JsonSerializable(typeof(FeedbinAddFeedRequest))]
[JsonSerializable(typeof(List<FeedbinTagging>))]
[JsonSerializable(typeof(List<FeedbinSubscription>))]
[JsonSerializable(typeof(List<FeedbinEntity>))]
[JsonSerializable(typeof(Models.GoogleReader.GoogleReaderAuthResponse))]
[JsonSerializable(typeof(Models.GoogleReader.GoogleReaderSubscriptionResponse))]
[JsonSerializable(typeof(Models.GoogleReader.GoogleReaderStreamContentResponse))]
[JsonSerializable(typeof(Models.Miniflux.MinifluxEntriesResponse))]
[JsonSerializable(typeof(List<Models.Miniflux.MinifluxFeed>))]
[JsonSerializable(typeof(Models.Miniflux.MinifluxAddFeedRequest))]
[JsonSerializable(typeof(Models.Miniflux.MinifluxFeedCreateResponse))]
[JsonSerializable(typeof(Models.Miniflux.MinifluxAddGroupRequest))]
[JsonSerializable(typeof(Models.Miniflux.MinifluxCategory))]
[JsonSerializable(typeof(Models.Miniflux.MinifluxMarkReadRequest))]
[JsonSerializable(typeof(Models.Miniflux.MinifluxUpdateFeedRequest))]
[JsonSerializable(typeof(Models.Miniflux.MinifluxUpdateGroupRequest))]
[JsonSerializable(typeof(Models.Inoreader.InoreaderFolderListResponse))]
[JsonSerializable(typeof(Models.Inoreader.InoreaderPreferenceResponse))]
[JsonSerializable(typeof(Models.Inoreader.InoreaderAuthResult))]
[JsonSerializable(typeof(Models.NewsBlur.NewsBlurAuthResult))]
[JsonSerializable(typeof(Models.NewsBlur.NewsBlurFeedResponse))]
[JsonSerializable(typeof(Models.NewsBlur.NewsBlurFeedListResponse))]
[JsonSerializable(typeof(Dictionary<string, NewsBlurFeed>))]
[JsonSerializable(typeof(RssClientConfiguration))]
internal sealed partial class JsonGenContext : JsonSerializerContext
{
}

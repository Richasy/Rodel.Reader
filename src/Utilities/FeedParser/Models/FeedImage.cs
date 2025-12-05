// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.FeedParser;

/// <summary>
/// Feed 图片.
/// </summary>
/// <param name="Url">图片地址.</param>
/// <param name="ImageType">图片类型.</param>
/// <param name="Title">图片标题（可选）.</param>
/// <param name="Description">图片描述（可选）.</param>
/// <param name="Link">关联链接（可选）.</param>
/// <param name="Width">图片宽度（可选）.</param>
/// <param name="Height">图片高度（可选）.</param>
public sealed record FeedImage(
    Uri Url,
    FeedImageType ImageType = FeedImageType.Logo,
    string? Title = null,
    string? Description = null,
    FeedLink? Link = null,
    int? Width = null,
    int? Height = null);

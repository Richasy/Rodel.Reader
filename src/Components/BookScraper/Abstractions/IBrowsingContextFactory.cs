// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.BookScraper.Abstractions;

/// <summary>
/// AngleSharp 浏览上下文工厂接口.
/// </summary>
public interface IBrowsingContextFactory
{
    /// <summary>
    /// 创建浏览上下文.
    /// </summary>
    /// <returns>浏览上下文.</returns>
    IBrowsingContext CreateContext();
}

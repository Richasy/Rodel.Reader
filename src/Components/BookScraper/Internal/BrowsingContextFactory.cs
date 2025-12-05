// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.BookScraper.Abstractions;

namespace Richasy.RodelReader.Components.BookScraper.Internal;

/// <summary>
/// AngleSharp 浏览上下文工厂实现.
/// </summary>
internal sealed class BrowsingContextFactory : IBrowsingContextFactory
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 初始化 <see cref="BrowsingContextFactory"/> 类的新实例.
    /// </summary>
    public BrowsingContextFactory()
    {
        _configuration = Configuration.Default.WithDefaultLoader();
    }

    /// <inheritdoc/>
    public IBrowsingContext CreateContext()
        => BrowsingContext.New(_configuration);
}

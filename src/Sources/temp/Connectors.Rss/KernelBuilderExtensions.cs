// Copyright (c) Reader Copilot. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Richasy.ReaderKernel.Connectors;
using Richasy.ReaderKernel.Connectors.Rss;
using Richasy.ReaderKernel.Models;
using RichasyKernel;

namespace Richasy.ReaderKernel;

/// <summary>
/// Kernel 构建器扩展.
/// </summary>
public static class KernelBuilderExtensions
{
    /// <summary>
    /// 添加本地 RSS 连接器.
    /// </summary>
    /// <returns><see cref="IKernelBuilder"/>.</returns>
    public static IKernelBuilder AddLocalRssConnector(this IKernelBuilder builder)
    {
        builder.Services.AddKeyedSingleton<IRssConnector, LocalRssConnector>(RssConnectorType.Local);
        return builder;
    }

    /// <summary>
    /// 添加 Feedbin RSS 连接器.
    /// </summary>
    /// <returns><see cref="IKernelBuilder"/>.</returns>
    public static IKernelBuilder AddFeedbinRssConnector(this IKernelBuilder builder)
    {
        builder.Services.AddKeyedSingleton<IRssConnector, FeedbinRssConnector>(RssConnectorType.Feedbin);
        return builder;
    }

    /// <summary>
    /// 添加 Google Reader RSS 连接器.
    /// </summary>
    /// <returns><see cref="IKernelBuilder"/>.</returns>
    public static IKernelBuilder AddGoogleReaderRssConnector(this IKernelBuilder builder)
    {
        builder.Services.AddKeyedSingleton<IRssConnector, GoogleReaderRssConnector>(RssConnectorType.GoogleReader);
        return builder;
    }

    /// <summary>
    /// 添加 Miniflux RSS 连接器.
    /// </summary>
    /// <returns><see cref="IKernelBuilder"/>.</returns>
    public static IKernelBuilder AddMinifluxRssConnector(this IKernelBuilder builder)
    {
        builder.Services.AddKeyedSingleton<IRssConnector, MinifluxRssConnector>(RssConnectorType.Miniflux);
        return builder;
    }

    /// <summary>
    /// 添加 Inoreader RSS 连接器.
    /// </summary>
    /// <returns><see cref="IKernelBuilder"/>.</returns>
    public static IKernelBuilder AddInoreaderRssConnector(this IKernelBuilder builder)
    {
        builder.Services.AddKeyedSingleton<IRssConnector, InoreaderRssConnector>(RssConnectorType.Inoreader);
        return builder;
    }

    /// <summary>
    /// 添加 NewsBlur RSS 连接器.
    /// </summary>
    /// <returns><see cref="IKernelBuilder"/>.</returns>
    public static IKernelBuilder AddNewsBlurRssConnector(this IKernelBuilder builder)
    {
        builder.Services.AddKeyedSingleton<IRssConnector, NewsBlurRssConnector>(RssConnectorType.NewsBlur);
        return builder;
    }
}

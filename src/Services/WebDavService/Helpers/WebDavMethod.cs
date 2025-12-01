// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV HTTP 方法.
/// </summary>
internal static class WebDavMethod
{
    /// <summary>
    /// PROPFIND 方法.
    /// </summary>
    public static readonly HttpMethod Propfind = new("PROPFIND");

    /// <summary>
    /// PROPPATCH 方法.
    /// </summary>
    public static readonly HttpMethod Proppatch = new("PROPPATCH");

    /// <summary>
    /// MKCOL 方法.
    /// </summary>
    public static readonly HttpMethod Mkcol = new("MKCOL");

    /// <summary>
    /// COPY 方法.
    /// </summary>
    public static readonly HttpMethod Copy = new("COPY");

    /// <summary>
    /// MOVE 方法.
    /// </summary>
    public static readonly HttpMethod Move = new("MOVE");

    /// <summary>
    /// LOCK 方法.
    /// </summary>
    public static readonly HttpMethod Lock = new("LOCK");

    /// <summary>
    /// UNLOCK 方法.
    /// </summary>
    public static readonly HttpMethod Unlock = new("UNLOCK");

    /// <summary>
    /// SEARCH 方法.
    /// </summary>
    public static readonly HttpMethod Search = new("SEARCH");
}

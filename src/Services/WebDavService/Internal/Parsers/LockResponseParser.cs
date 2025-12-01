// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// 锁响应解析器.
/// </summary>
internal sealed class LockResponseParser : IResponseParser<LockResponse>
{
    /// <inheritdoc/>
    public LockResponse Parse(string responseContent, int statusCode, string? description)
    {
        if (!XmlExtensions.TryParse(responseContent, out var document) || document?.Root == null)
        {
            return new LockResponse(statusCode, description);
        }

        var activeLock = ParseActiveLock(document.Root);
        return new LockResponse(statusCode, description, activeLock);
    }

    /// <summary>
    /// 从元素中解析活动锁.
    /// </summary>
    public static ActiveLock? ParseActiveLock(XElement element)
    {
        var lockDiscovery = element.LocalElement("lockdiscovery") ?? element;
        var activeLockElement = lockDiscovery.LocalElement("activelock");

        if (activeLockElement == null)
        {
            return null;
        }

        var lockScope = ParseLockScope(activeLockElement.LocalElement("lockscope"));
        var lockToken = activeLockElement.LocalElement("locktoken")?.LocalElement("href")?.Value;
        var depth = activeLockElement.LocalElement("depth")?.Value;
        var timeout = activeLockElement.LocalElement("timeout")?.Value;
        var owner = ParseLockOwner(activeLockElement.LocalElement("owner"));
        var lockRoot = activeLockElement.LocalElement("lockroot")?.LocalElement("href")?.Value;

        return new ActiveLock
        {
            LockScope = lockScope,
            LockToken = lockToken,
            Depth = depth,
            Timeout = timeout,
            Owner = owner,
            LockRoot = lockRoot,
        };
    }

    /// <summary>
    /// 解析活动锁集合.
    /// </summary>
    public static IReadOnlyCollection<ActiveLock> ParseActiveLocks(XElement? lockDiscoveryElement)
    {
        if (lockDiscoveryElement == null)
        {
            return [];
        }

        var locks = new List<ActiveLock>();
        foreach (var activeLockElement in lockDiscoveryElement.LocalElements("activelock"))
        {
            var lockScope = ParseLockScope(activeLockElement.LocalElement("lockscope"));
            var lockToken = activeLockElement.LocalElement("locktoken")?.LocalElement("href")?.Value;
            var depth = activeLockElement.LocalElement("depth")?.Value;
            var timeout = activeLockElement.LocalElement("timeout")?.Value;
            var owner = ParseLockOwner(activeLockElement.LocalElement("owner"));
            var lockRoot = activeLockElement.LocalElement("lockroot")?.LocalElement("href")?.Value;

            locks.Add(new ActiveLock
            {
                LockScope = lockScope,
                LockToken = lockToken,
                Depth = depth,
                Timeout = timeout,
                Owner = owner,
                LockRoot = lockRoot,
            });
        }

        return locks;
    }

    private static LockScope ParseLockScope(XElement? scopeElement)
    {
        if (scopeElement == null)
        {
            return LockScope.Shared;
        }

        return scopeElement.LocalElement("exclusive") != null
            ? LockScope.Exclusive
            : LockScope.Shared;
    }

    private static LockOwner? ParseLockOwner(XElement? ownerElement)
    {
        if (ownerElement == null)
        {
            return null;
        }

        var href = ownerElement.LocalElement("href")?.Value;
        if (!string.IsNullOrEmpty(href) && Uri.TryCreate(href, UriKind.Absolute, out var uri))
        {
            return new UriLockOwner(uri);
        }

        var value = ownerElement.Value;
        if (!string.IsNullOrEmpty(value))
        {
            return new PrincipalLockOwner(value);
        }

        return null;
    }
}

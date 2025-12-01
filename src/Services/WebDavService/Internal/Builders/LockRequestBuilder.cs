// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// LOCK 请求构建器.
/// </summary>
internal static class LockRequestBuilder
{
    private static readonly XNamespace DavNs = WebDavConstants.DavNamespace;

    /// <summary>
    /// 构建 LOCK 请求体.
    /// </summary>
    /// <param name="parameters">锁参数.</param>
    /// <returns>请求体 XML 字符串.</returns>
    public static string BuildRequestBody(LockParameters parameters)
    {
        var root = new XElement(DavNs + "lockinfo",
            new XAttribute(XNamespace.Xmlns + "D", DavNs.NamespaceName));

        // 锁定范围
        var lockScope = new XElement(DavNs + "lockscope");
        lockScope.Add(parameters.LockScope == LockScope.Exclusive
            ? new XElement(DavNs + "exclusive")
            : new XElement(DavNs + "shared"));
        root.Add(lockScope);

        // 锁定类型（总是 write）
        var lockType = new XElement(DavNs + "locktype",
            new XElement(DavNs + "write"));
        root.Add(lockType);

        // 锁定所有者
        if (parameters.Owner != null)
        {
            var owner = new XElement(DavNs + "owner");

            if (parameters.Owner is UriLockOwner uriOwner)
            {
                owner.Add(new XElement(DavNs + "href", uriOwner.Uri.ToString()));
            }
            else
            {
                owner.Value = parameters.Owner.Value;
            }

            root.Add(owner);
        }

        return root.ToString(SaveOptions.DisableFormatting);
    }
}

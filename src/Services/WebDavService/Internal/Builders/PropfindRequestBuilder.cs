// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// PROPFIND 请求构建器.
/// </summary>
internal static class PropfindRequestBuilder
{
    private static readonly XNamespace DavNs = WebDavConstants.DavNamespace;

    /// <summary>
    /// 构建 PROPFIND 请求体.
    /// </summary>
    /// <param name="requestType">请求类型.</param>
    /// <param name="customProperties">自定义属性.</param>
    /// <param name="namespaces">自定义命名空间.</param>
    /// <returns>请求体 XML 字符串.</returns>
    public static string BuildRequest(
        PropfindRequestType requestType,
        IReadOnlyCollection<WebDavProperty>? customProperties,
        IReadOnlyCollection<NamespaceAttribute>? namespaces)
    {
        var root = new XElement(DavNs + "propfind",
            new XAttribute(XNamespace.Xmlns + "D", DavNs.NamespaceName));

        // 添加自定义命名空间
        if (namespaces != null)
        {
            foreach (var ns in namespaces)
            {
                root.Add(new XAttribute(XNamespace.Xmlns + ns.Prefix, ns.Namespace));
            }
        }

        switch (requestType)
        {
            case PropfindRequestType.AllProperties:
                root.Add(new XElement(DavNs + "allprop"));
                break;

            case PropfindRequestType.NamedProperties:
                var prop = new XElement(DavNs + "prop");
                if (customProperties != null)
                {
                    foreach (var property in customProperties)
                    {
                        XNamespace propNs = property.Namespace ?? DavNs.NamespaceName;
                        prop.Add(new XElement(propNs + property.Name));
                    }
                }

                root.Add(prop);
                break;

            case PropfindRequestType.AllPropertiesImplied:
            default:
                // 空请求体
                break;
        }

        return root.ToString(SaveOptions.DisableFormatting);
    }
}

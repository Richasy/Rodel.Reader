// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// PROPPATCH 请求构建器.
/// </summary>
internal static class ProppatchRequestBuilder
{
    private static readonly XNamespace DavNs = WebDavConstants.DavNamespace;

    /// <summary>
    /// 构建 PROPPATCH 请求体.
    /// </summary>
    /// <param name="propertiesToSet">要设置的属性.</param>
    /// <param name="propertiesToRemove">要删除的属性.</param>
    /// <param name="namespaces">自定义命名空间.</param>
    /// <returns>请求体 XML 字符串.</returns>
    public static string BuildRequestBody(
        IReadOnlyCollection<WebDavProperty>? propertiesToSet,
        IReadOnlyCollection<WebDavProperty>? propertiesToRemove,
        IReadOnlyCollection<NamespaceAttribute>? namespaces)
    {
        var root = new XElement(DavNs + "propertyupdate",
            new XAttribute(XNamespace.Xmlns + "D", DavNs.NamespaceName));

        // 添加自定义命名空间
        if (namespaces != null)
        {
            foreach (var ns in namespaces)
            {
                root.Add(new XAttribute(XNamespace.Xmlns + ns.Prefix, ns.Namespace));
            }
        }

        // 添加要设置的属性
        if (propertiesToSet != null && propertiesToSet.Count > 0)
        {
            var set = new XElement(DavNs + "set");
            var prop = new XElement(DavNs + "prop");

            foreach (var property in propertiesToSet)
            {
                XNamespace propNs = property.Namespace ?? DavNs.NamespaceName;
                prop.Add(new XElement(propNs + property.Name, property.Value));
            }

            set.Add(prop);
            root.Add(set);
        }

        // 添加要删除的属性
        if (propertiesToRemove != null && propertiesToRemove.Count > 0)
        {
            var remove = new XElement(DavNs + "remove");
            var prop = new XElement(DavNs + "prop");

            foreach (var property in propertiesToRemove)
            {
                XNamespace propNs = property.Namespace ?? DavNs.NamespaceName;
                prop.Add(new XElement(propNs + property.Name));
            }

            remove.Add(prop);
            root.Add(remove);
        }

        return root.ToString(SaveOptions.DisableFormatting);
    }
}

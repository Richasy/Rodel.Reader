// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// SEARCH 请求构建器.
/// </summary>
internal static class SearchRequestBuilder
{
    private static readonly XNamespace DavNs = WebDavConstants.DavNamespace;

    /// <summary>
    /// 构建 SEARCH 请求体.
    /// </summary>
    /// <param name="parameters">搜索参数.</param>
    /// <returns>请求体 XML 字符串.</returns>
    public static string BuildRequestBody(SearchParameters parameters)
    {
        // 如果提供了原始请求，直接返回
        if (!string.IsNullOrEmpty(parameters.RawSearchRequest))
        {
            return parameters.RawSearchRequest;
        }

        // 构建基本的 DASL 搜索请求
        var root = new XElement(DavNs + "searchrequest",
            new XAttribute(XNamespace.Xmlns + "D", DavNs.NamespaceName));

        var basicSearch = new XElement(DavNs + "basicsearch");

        // 选择所有属性
        var select = new XElement(DavNs + "select",
            new XElement(DavNs + "allprop"));
        basicSearch.Add(select);

        // 搜索范围
        var from = new XElement(DavNs + "from",
            new XElement(DavNs + "scope",
                new XElement(DavNs + "href", parameters.SearchScope ?? "/"),
                new XElement(DavNs + "depth", "infinity")));
        basicSearch.Add(from);

        // 搜索条件（简单的 displayname 包含搜索）
        if (!string.IsNullOrEmpty(parameters.Keyword))
        {
            var where = new XElement(DavNs + "where",
                new XElement(DavNs + "like",
                    new XElement(DavNs + "prop",
                        new XElement(DavNs + "displayname")),
                    new XElement(DavNs + "literal", $"%{parameters.Keyword}%")));
            basicSearch.Add(where);
        }

        root.Add(basicSearch);

        return root.ToString(SaveOptions.DisableFormatting);
    }
}

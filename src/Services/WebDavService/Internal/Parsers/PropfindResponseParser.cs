// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// PROPFIND 响应解析器.
/// </summary>
internal sealed class PropfindResponseParser : IResponseParser<PropfindResponse>
{
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="PropfindResponseParser"/> 类的新实例.
    /// </summary>
    /// <param name="logger">日志器.</param>
    public PropfindResponseParser(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc/>
    public PropfindResponse Parse(string responseContent, int statusCode, string? description)
    {
        if (!XmlExtensions.TryParse(responseContent, out var document) || document?.Root == null)
        {
            _logger.LogWarning("Failed to parse PROPFIND response XML");
            return new PropfindResponse(statusCode, description);
        }

        var resources = new List<WebDavResource>();
        var responseElements = document.Root.LocalElements("response");

        foreach (var responseElement in responseElements)
        {
            try
            {
                var resource = ParseResource(responseElement);
                if (resource != null)
                {
                    resources.Add(resource);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse resource element");
            }
        }

        _logger.LogDebug("Parsed {Count} resources from PROPFIND response", resources.Count);
        return new PropfindResponse(statusCode, description, resources);
    }

    private static WebDavResource? ParseResource(XElement responseElement)
    {
        var href = responseElement.LocalElement("href")?.Value;
        if (string.IsNullOrEmpty(href))
        {
            return null;
        }

        var resource = new WebDavResource
        {
            Uri = Uri.UnescapeDataString(href),
        };

        var propstatElements = responseElement.LocalElements("propstat");
        var allProperties = new List<WebDavProperty>();
        var allPropertyStatuses = new List<WebDavPropertyStatus>();

        foreach (var propstatElement in propstatElements)
        {
            var status = propstatElement.LocalElement("status")?.Value;
            var propStatusCode = PropertyValueParser.ParseStatusCode(status);

            var propElement = propstatElement.LocalElement("prop");
            if (propElement == null)
            {
                continue;
            }

            // 解析标准属性
            ParseStandardProperties(propElement, resource, href);

            // 解析所有属性
            foreach (var prop in propElement.Elements())
            {
                var property = new WebDavProperty(
                    prop.Name.LocalName,
                    prop.Name.NamespaceName,
                    prop.Value);

                allProperties.Add(property);
                allPropertyStatuses.Add(new WebDavPropertyStatus(property, propStatusCode, status));
            }

            // 解析锁信息
            var lockDiscovery = propElement.LocalElement("lockdiscovery");
            if (lockDiscovery != null)
            {
                resource.ActiveLocks = LockResponseParser.ParseActiveLocks(lockDiscovery);
            }
        }

        resource.Properties = allProperties;
        resource.PropertyStatuses = allPropertyStatuses;

        return resource;
    }

    private static void ParseStandardProperties(XElement propElement, WebDavResource resource, string href)
    {
        // 显示名称
        var displayName = propElement.LocalElement("displayname")?.Value;
        resource.DisplayName = string.IsNullOrEmpty(displayName)
            ? Uri.UnescapeDataString(href.Split('/').LastOrDefault(p => !string.IsNullOrEmpty(p)) ?? string.Empty)
            : displayName;

        // 是否为集合
        var resourceType = propElement.LocalElement("resourcetype");
        resource.IsCollection = PropertyValueParser.IsCollection(resourceType);

        // 是否隐藏
        var isHidden = propElement.LocalElement("ishidden")?.Value;
        resource.IsHidden = PropertyValueParser.ParseBool(isHidden);

        // 内容长度
        var contentLength = propElement.LocalElement("getcontentlength")?.Value;
        resource.ContentLength = PropertyValueParser.ParseLong(contentLength);

        // 内容类型
        resource.ContentType = propElement.LocalElement("getcontenttype")?.Value;

        // 内容语言
        resource.ContentLanguage = propElement.LocalElement("getcontentlanguage")?.Value;

        // 创建日期
        var creationDate = propElement.LocalElement("creationdate")?.Value;
        resource.CreationDate = PropertyValueParser.ParseDateTime(creationDate);

        // 最后修改日期
        var lastModified = propElement.LocalElement("getlastmodified")?.Value;
        resource.LastModifiedDate = PropertyValueParser.ParseDateTime(lastModified);

        // ETag
        resource.ETag = propElement.LocalElement("getetag")?.Value;
    }
}

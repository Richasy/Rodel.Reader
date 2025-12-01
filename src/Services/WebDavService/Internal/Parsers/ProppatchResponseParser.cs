// Copyright (c) Richasy. All rights reserved.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// PROPPATCH 响应解析器.
/// </summary>
internal sealed class ProppatchResponseParser : IResponseParser<ProppatchResponse>
{
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="ProppatchResponseParser"/> 类的新实例.
    /// </summary>
    /// <param name="logger">日志器.</param>
    public ProppatchResponseParser(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc/>
    public ProppatchResponse Parse(string responseContent, int statusCode, string? description)
    {
        if (!XmlExtensions.TryParse(responseContent, out var document) || document?.Root == null)
        {
            _logger.LogWarning("Failed to parse PROPPATCH response XML");
            return new ProppatchResponse(statusCode, description);
        }

        var propertyStatuses = new List<WebDavPropertyStatus>();
        var responseElements = document.Root.LocalElements("response");

        foreach (var responseElement in responseElements)
        {
            var propstatElements = responseElement.LocalElements("propstat");

            foreach (var propstatElement in propstatElements)
            {
                var status = propstatElement.LocalElement("status")?.Value;
                var propStatusCode = PropertyValueParser.ParseStatusCode(status);

                var propElement = propstatElement.LocalElement("prop");
                if (propElement == null)
                {
                    continue;
                }

                foreach (var prop in propElement.Elements())
                {
                    var property = new WebDavProperty(
                        prop.Name.LocalName,
                        prop.Name.NamespaceName,
                        prop.Value);

                    propertyStatuses.Add(new WebDavPropertyStatus(property, propStatusCode, status));
                }
            }
        }

        _logger.LogDebug("Parsed {Count} property statuses from PROPPATCH response", propertyStatuses.Count);
        return new ProppatchResponse(statusCode, description, propertyStatuses);
    }
}

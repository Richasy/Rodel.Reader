// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav.Test;

/// <summary>
/// 测试数据工厂.
/// </summary>
internal static class TestDataFactory
{
    public const string BaseAddress = "https://webdav.example.com";

    public static string PropfindResponse => """
        <?xml version="1.0" encoding="utf-8"?>
        <D:multistatus xmlns:D="DAV:">
            <D:response>
                <D:href>/documents/</D:href>
                <D:propstat>
                    <D:prop>
                        <D:displayname>documents</D:displayname>
                        <D:resourcetype><D:collection/></D:resourcetype>
                        <D:getlastmodified>Mon, 01 Jan 2024 12:00:00 GMT</D:getlastmodified>
                        <D:creationdate>2024-01-01T00:00:00Z</D:creationdate>
                    </D:prop>
                    <D:status>HTTP/1.1 200 OK</D:status>
                </D:propstat>
            </D:response>
            <D:response>
                <D:href>/documents/file.txt</D:href>
                <D:propstat>
                    <D:prop>
                        <D:displayname>file.txt</D:displayname>
                        <D:resourcetype/>
                        <D:getcontentlength>1024</D:getcontentlength>
                        <D:getcontenttype>text/plain</D:getcontenttype>
                        <D:getlastmodified>Mon, 01 Jan 2024 12:00:00 GMT</D:getlastmodified>
                        <D:getetag>"abc123"</D:getetag>
                    </D:prop>
                    <D:status>HTTP/1.1 200 OK</D:status>
                </D:propstat>
            </D:response>
        </D:multistatus>
        """;

    public static string ProppatchResponse => """
        <?xml version="1.0" encoding="utf-8"?>
        <D:multistatus xmlns:D="DAV:">
            <D:response>
                <D:href>/documents/file.txt</D:href>
                <D:propstat>
                    <D:prop>
                        <D:displayname/>
                    </D:prop>
                    <D:status>HTTP/1.1 200 OK</D:status>
                </D:propstat>
            </D:response>
        </D:multistatus>
        """;

    public static string LockResponse => """
        <?xml version="1.0" encoding="utf-8"?>
        <D:prop xmlns:D="DAV:">
            <D:lockdiscovery>
                <D:activelock>
                    <D:locktype><D:write/></D:locktype>
                    <D:lockscope><D:exclusive/></D:lockscope>
                    <D:depth>infinity</D:depth>
                    <D:owner>
                        <D:href>mailto:user@example.com</D:href>
                    </D:owner>
                    <D:timeout>Second-3600</D:timeout>
                    <D:locktoken>
                        <D:href>urn:uuid:a515cfa4-5d2a-11d1-8f23-00aa00bd5301</D:href>
                    </D:locktoken>
                    <D:lockroot>
                        <D:href>/documents/file.txt</D:href>
                    </D:lockroot>
                </D:activelock>
            </D:lockdiscovery>
        </D:prop>
        """;

    public static string SearchResponse => """
        <?xml version="1.0" encoding="utf-8"?>
        <D:multistatus xmlns:D="DAV:">
            <D:response>
                <D:href>/documents/result.txt</D:href>
                <D:propstat>
                    <D:prop>
                        <D:displayname>result.txt</D:displayname>
                        <D:resourcetype/>
                    </D:prop>
                    <D:status>HTTP/1.1 200 OK</D:status>
                </D:propstat>
            </D:response>
        </D:multistatus>
        """;

    public static HttpClient CreateMockHttpClient(MockHttpMessageHandler handler)
    {
        var client = handler.ToHttpClient();
        client.BaseAddress = new Uri(BaseAddress);
        return client;
    }
}

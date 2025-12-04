// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.Legado.Internal;

namespace Richasy.RodelReader.Sources.Legado.Test.Unit;

/// <summary>
/// ApiEndpoints 单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class ApiEndpointsTests
{
    [TestMethod]
    public void BuildUrl_WithLegadoServerType_NoPrefix()
    {
        // Arrange
        var baseUrl = "http://localhost:1234";
        var endpoint = "getBookshelf";

        // Act
        var url = ApiEndpoints.BuildUrl(baseUrl, endpoint, ServerType.Legado);

        // Assert
        Assert.AreEqual("http://localhost:1234/getBookshelf", url);
    }

    [TestMethod]
    public void BuildUrl_WithHectorqinReader_HasReader3Prefix()
    {
        // Arrange
        var baseUrl = "http://localhost:8080";
        var endpoint = "getBookshelf";

        // Act
        var url = ApiEndpoints.BuildUrl(baseUrl, endpoint, ServerType.HectorqinReader);

        // Assert
        Assert.AreEqual("http://localhost:8080/reader3/getBookshelf", url);
    }

    [TestMethod]
    public void BuildUrl_WithAccessToken_IncludesToken()
    {
        // Arrange
        var baseUrl = "http://localhost:1234";
        var endpoint = "getBookshelf";
        var accessToken = "my-token";

        // Act
        var url = ApiEndpoints.BuildUrl(baseUrl, endpoint, ServerType.Legado, accessToken);

        // Assert
        Assert.IsTrue(url.Contains("accessToken=my-token", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildUrl_WithQueryParams_IncludesParams()
    {
        // Arrange
        var baseUrl = "http://localhost:1234";
        var endpoint = "getChapterList";
        var queryParams = new Dictionary<string, string>
        {
            { "url", "http://example.com/book" },
        };

        // Act
        var url = ApiEndpoints.BuildUrl(baseUrl, endpoint, ServerType.Legado, null, queryParams);

        // Assert
        Assert.IsTrue(url.Contains("url=", StringComparison.Ordinal));
        Assert.IsTrue(url.Contains("example.com", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildUrl_WithTokenAndParams_IncludesBoth()
    {
        // Arrange
        var baseUrl = "http://localhost:8080";
        var endpoint = "getBookContent";
        var accessToken = "token123";
        var queryParams = new Dictionary<string, string>
        {
            { "url", "http://test.com" },
            { "index", "5" },
        };

        // Act
        var url = ApiEndpoints.BuildUrl(baseUrl, endpoint, ServerType.HectorqinReader, accessToken, queryParams);

        // Assert
        Assert.IsTrue(url.StartsWith("http://localhost:8080/reader3/getBookContent", StringComparison.Ordinal));
        Assert.IsTrue(url.Contains("accessToken=token123", StringComparison.Ordinal));
        Assert.IsTrue(url.Contains("url=", StringComparison.Ordinal));
        Assert.IsTrue(url.Contains("index=5", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildUrl_WithTrailingSlash_NormalizesUrl()
    {
        // Arrange
        var baseUrl = "http://localhost:1234/";
        var endpoint = "getBookshelf";

        // Act
        var url = ApiEndpoints.BuildUrl(baseUrl, endpoint, ServerType.Legado);

        // Assert
        Assert.AreEqual("http://localhost:1234/getBookshelf", url);
        Assert.IsFalse(url.Contains("//getBookshelf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildUrl_WithSpecialCharacters_EncodesCorrectly()
    {
        // Arrange
        var baseUrl = "http://localhost:1234";
        var endpoint = "getChapterList";
        var queryParams = new Dictionary<string, string>
        {
            { "url", "http://example.com/book?id=1&name=测试" },
        };

        // Act
        var url = ApiEndpoints.BuildUrl(baseUrl, endpoint, ServerType.Legado, null, queryParams);

        // Assert
        Assert.IsTrue(url.Contains("url=", StringComparison.Ordinal));
        // URL 编码后不应包含原始的 & 符号（除了参数分隔符）
        Assert.IsFalse(url.Contains("&name=", StringComparison.Ordinal));
    }
}

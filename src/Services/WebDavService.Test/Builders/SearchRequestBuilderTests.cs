// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav.Test.Builders;

[TestClass]
public class SearchRequestBuilderTests
{
    [TestMethod]
    public void BuildRequestBody_ContainsSearchRequest()
    {
        // Arrange
        var parameters = new SearchParameters { Keyword = "test query" };

        // Act
        var result = SearchRequestBuilder.BuildRequestBody(parameters);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("searchrequest", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void BuildRequestBody_ContainsSearchKeyword()
    {
        // Arrange
        var parameters = new SearchParameters { Keyword = "important document" };

        // Act
        var result = SearchRequestBuilder.BuildRequestBody(parameters);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("important document", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildRequestBody_ContainsScope()
    {
        // Arrange
        var parameters = new SearchParameters
        {
            Keyword = "query",
            SearchScope = "/documents/folder",
        };

        // Act
        var result = SearchRequestBuilder.BuildRequestBody(parameters);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("/documents/folder", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildRequestBody_WithRawRequest_ReturnsRawRequest()
    {
        // Arrange
        var rawRequest = "<custom>search request</custom>";
        var parameters = new SearchParameters { RawSearchRequest = rawRequest };

        // Act
        var result = SearchRequestBuilder.BuildRequestBody(parameters);

        // Assert
        Assert.AreEqual(rawRequest, result);
    }

    [TestMethod]
    public void BuildRequestBody_ContainsBasicSearch()
    {
        // Arrange
        var parameters = new SearchParameters { Keyword = "test" };

        // Act
        var result = SearchRequestBuilder.BuildRequestBody(parameters);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("basicsearch", StringComparison.OrdinalIgnoreCase));
    }
}

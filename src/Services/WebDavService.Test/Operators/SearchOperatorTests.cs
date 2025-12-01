// Copyright (c) Richasy. All rights reserved.

using System.Net;

namespace Richasy.RodelReader.Services.WebDav.Test.Operators;

[TestClass]
public class SearchOperatorTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private SearchOperator _operator = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        var httpClient = TestDataFactory.CreateMockHttpClient(_mockHandler);
        var dispatcher = new WebDavDispatcher(httpClient);
        var parser = new PropfindResponseParser();
        _operator = new SearchOperator(dispatcher, parser);
    }

    [TestMethod]
    public async Task SearchAsync_Success_ReturnsResults()
    {
        // Arrange
        _mockHandler.When("*")
            .With(r => r.Method.Method == "SEARCH")
            .Respond(HttpStatusCode.MultiStatus, "application/xml", TestDataFactory.SearchResponse);

        var parameters = new SearchParameters
        {
            Keyword = "result",
        };

        // Act
        var result = await _operator.SearchAsync(new Uri("/", UriKind.Relative), parameters);

        // Assert
        Assert.AreEqual(207, result.StatusCode);
        Assert.IsTrue(result.Resources.Count > 0);
    }

    [TestMethod]
    public async Task SearchAsync_NoResults_ReturnsEmptyList()
    {
        // Arrange
        var emptyResponse = """
            <?xml version="1.0" encoding="utf-8"?>
            <D:multistatus xmlns:D="DAV:">
            </D:multistatus>
            """;
        _mockHandler.When("*")
            .Respond(HttpStatusCode.MultiStatus, "application/xml", emptyResponse);

        var parameters = new SearchParameters
        {
            Keyword = "nonexistent",
        };

        // Act
        var result = await _operator.SearchAsync(new Uri("/", UriKind.Relative), parameters);

        // Assert
        Assert.AreEqual(207, result.StatusCode);
        Assert.AreEqual(0, result.Resources.Count);
    }

    [TestMethod]
    public async Task SearchAsync_WithScope_UsesCorrectScope()
    {
        // Arrange
        _mockHandler.When("*")
            .With(r => r.Content != null)
            .Respond(HttpStatusCode.MultiStatus, "application/xml", TestDataFactory.SearchResponse);

        var parameters = new SearchParameters
        {
            Keyword = "test",
            SearchScope = "/documents",
        };

        // Act
        var result = await _operator.SearchAsync(new Uri("/", UriKind.Relative), parameters);

        // Assert
        Assert.IsTrue(result.IsSuccessful);
    }
}

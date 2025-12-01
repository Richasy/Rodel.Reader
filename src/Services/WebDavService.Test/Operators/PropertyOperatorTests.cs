// Copyright (c) Richasy. All rights reserved.

using System.Net;

namespace Richasy.RodelReader.Services.WebDav.Test.Operators;

[TestClass]
public class PropertyOperatorTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private PropertyOperator _operator = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        var httpClient = TestDataFactory.CreateMockHttpClient(_mockHandler);
        var dispatcher = new WebDavDispatcher(httpClient);
        var propfindParser = new PropfindResponseParser();
        var proppatchParser = new ProppatchResponseParser();
        _operator = new PropertyOperator(dispatcher, propfindParser, proppatchParser);
    }

    [TestMethod]
    public async Task PropfindAsync_Success_ReturnsResources()
    {
        // Arrange
        _mockHandler.When("*")
            .Respond(HttpStatusCode.MultiStatus, "application/xml", TestDataFactory.PropfindResponse);

        var parameters = new PropfindParameters
        {
            RequestType = PropfindRequestType.AllProperties,
            ApplyTo = ApplyTo.Propfind.ResourceAndChildren,
        };

        // Act
        var result = await _operator.PropfindAsync(new Uri("/documents", UriKind.Relative), parameters);

        // Assert
        Assert.AreEqual(207, result.StatusCode);
        Assert.IsTrue(result.Resources.Count > 0);
    }

    [TestMethod]
    public async Task PropfindAsync_NotFound_Returns404()
    {
        // Arrange
        _mockHandler.When("*")
            .Respond(HttpStatusCode.NotFound);

        var parameters = new PropfindParameters
        {
            RequestType = PropfindRequestType.AllProperties,
        };

        // Act
        var result = await _operator.PropfindAsync(new Uri("/nonexistent", UriKind.Relative), parameters);

        // Assert
        Assert.AreEqual(404, result.StatusCode);
    }

    [TestMethod]
    public async Task ProppatchAsync_Success_ReturnsOk()
    {
        // Arrange
        _mockHandler.When("*")
            .Respond(HttpStatusCode.MultiStatus, "application/xml", TestDataFactory.ProppatchResponse);

        var parameters = new ProppatchParameters
        {
            PropertiesToSet = new[]
            {
                new WebDavProperty("displayname", WebDavConstants.DavNamespace, "New Name"),
            },
        };

        // Act
        var result = await _operator.ProppatchAsync(new Uri("/documents/file.txt", UriKind.Relative), parameters);

        // Assert
        Assert.AreEqual(207, result.StatusCode);
    }
}

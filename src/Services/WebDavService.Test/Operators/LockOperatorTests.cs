// Copyright (c) Richasy. All rights reserved.

using System.Net;

namespace Richasy.RodelReader.Services.WebDav.Test.Operators;

[TestClass]
public class LockOperatorTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private LockOperator _operator = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        var httpClient = TestDataFactory.CreateMockHttpClient(_mockHandler);
        var dispatcher = new WebDavDispatcher(httpClient);
        var lockParser = new LockResponseParser();
        _operator = new LockOperator(dispatcher, lockParser);
    }

    [TestMethod]
    public async Task LockAsync_Success_ReturnsLockToken()
    {
        // Arrange
        _mockHandler.When("*")
            .With(r => r.Method.Method == "LOCK")
            .Respond(HttpStatusCode.OK, "application/xml", TestDataFactory.LockResponse);

        var parameters = new LockParameters
        {
            LockScope = LockScope.Exclusive,
        };

        // Act
        var result = await _operator.LockAsync(new Uri("/file.txt", UriKind.Relative), parameters);

        // Assert
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsNotNull(result.ActiveLock);
        Assert.IsNotNull(result.LockToken);
    }

    [TestMethod]
    public async Task LockAsync_ExclusiveLock_ParsesScope()
    {
        // Arrange
        _mockHandler.When("*")
            .Respond(HttpStatusCode.OK, "application/xml", TestDataFactory.LockResponse);

        var parameters = new LockParameters
        {
            LockScope = LockScope.Exclusive,
        };

        // Act
        var result = await _operator.LockAsync(new Uri("/file.txt", UriKind.Relative), parameters);

        // Assert
        Assert.IsNotNull(result.ActiveLock);
        Assert.AreEqual(LockScope.Exclusive, result.ActiveLock.LockScope);
    }

    [TestMethod]
    public async Task LockAsync_Conflict_Returns423()
    {
        // Arrange
        _mockHandler.When("*")
            .Respond(HttpStatusCode.Locked);

        var parameters = new LockParameters
        {
            LockScope = LockScope.Exclusive,
        };

        // Act
        var result = await _operator.LockAsync(new Uri("/locked-file.txt", UriKind.Relative), parameters);

        // Assert
        Assert.AreEqual(423, result.StatusCode);
    }

    [TestMethod]
    public async Task UnlockAsync_Success_ReturnsNoContent()
    {
        // Arrange
        _mockHandler.When("*")
            .With(r => r.Method.Method == "UNLOCK")
            .Respond(HttpStatusCode.NoContent);

        var parameters = new UnlockParameters("urn:uuid:a515cfa4-5d2a-11d1-8f23-00aa00bd5301");

        // Act
        var result = await _operator.UnlockAsync(new Uri("/file.txt", UriKind.Relative), parameters);

        // Assert
        Assert.AreEqual(204, result.StatusCode);
    }

    [TestMethod]
    public async Task UnlockAsync_InvalidToken_Returns412()
    {
        // Arrange
        _mockHandler.When("*")
            .Respond(HttpStatusCode.PreconditionFailed);

        var parameters = new UnlockParameters("invalid-token");

        // Act
        var result = await _operator.UnlockAsync(new Uri("/file.txt", UriKind.Relative), parameters);

        // Assert
        Assert.AreEqual(412, result.StatusCode);
    }
}

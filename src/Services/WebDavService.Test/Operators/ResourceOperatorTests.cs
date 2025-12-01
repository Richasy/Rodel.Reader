// Copyright (c) Richasy. All rights reserved.

using System.Net;

namespace Richasy.RodelReader.Services.WebDav.Test.Operators;

[TestClass]
public class ResourceOperatorTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private ResourceOperator _operator = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        var httpClient = TestDataFactory.CreateMockHttpClient(_mockHandler);
        var dispatcher = new WebDavDispatcher(httpClient);
        _operator = new ResourceOperator(dispatcher);
    }

    [TestMethod]
    public async Task MkColAsync_Success_ReturnsCreated()
    {
        // Arrange
        _mockHandler.When("*")
            .With(r => r.Method.Method == "MKCOL")
            .Respond(HttpStatusCode.Created);

        // Act
        var result = await _operator.MkColAsync(new Uri("/new-folder", UriKind.Relative));

        // Assert
        Assert.AreEqual(201, result.StatusCode);
    }

    [TestMethod]
    public async Task MkColAsync_AlreadyExists_ReturnsMethodNotAllowed()
    {
        // Arrange
        _mockHandler.When("*")
            .Respond(HttpStatusCode.MethodNotAllowed);

        // Act
        var result = await _operator.MkColAsync(new Uri("/existing-folder", UriKind.Relative));

        // Assert
        Assert.AreEqual(405, result.StatusCode);
    }

    [TestMethod]
    public async Task DeleteAsync_Success_ReturnsNoContent()
    {
        // Arrange
        _mockHandler.When(HttpMethod.Delete, "*")
            .Respond(HttpStatusCode.NoContent);

        // Act
        var result = await _operator.DeleteAsync(new Uri("/old-file.txt", UriKind.Relative));

        // Assert
        Assert.AreEqual(204, result.StatusCode);
    }

    [TestMethod]
    public async Task DeleteAsync_NotFound_Returns404()
    {
        // Arrange
        _mockHandler.When(HttpMethod.Delete, "*")
            .Respond(HttpStatusCode.NotFound);

        // Act
        var result = await _operator.DeleteAsync(new Uri("/nonexistent.txt", UriKind.Relative));

        // Assert
        Assert.AreEqual(404, result.StatusCode);
    }

    [TestMethod]
    public async Task CopyAsync_Success_ReturnsCreated()
    {
        // Arrange
        _mockHandler.When("*")
            .With(r => r.Method.Method == "COPY")
            .Respond(HttpStatusCode.Created);

        // Act
        var result = await _operator.CopyAsync(
            new Uri("/source.txt", UriKind.Relative),
            new Uri("/dest.txt", UriKind.Relative));

        // Assert
        Assert.AreEqual(201, result.StatusCode);
    }

    [TestMethod]
    public async Task MoveAsync_Success_ReturnsCreated()
    {
        // Arrange
        _mockHandler.When("*")
            .With(r => r.Method.Method == "MOVE")
            .Respond(HttpStatusCode.Created);

        // Act
        var result = await _operator.MoveAsync(
            new Uri("/old-name.txt", UriKind.Relative),
            new Uri("/new-name.txt", UriKind.Relative));

        // Assert
        Assert.AreEqual(201, result.StatusCode);
    }

    [TestMethod]
    public async Task MoveAsync_WithOverwrite_SetsOverwriteHeader()
    {
        // Arrange
        _mockHandler.When("*")
            .With(r => r.Headers.Contains("Overwrite") && r.Headers.GetValues("Overwrite").First() == "T")
            .Respond(HttpStatusCode.NoContent);

        var parameters = new MoveParameters { Overwrite = true };

        // Act
        var result = await _operator.MoveAsync(
            new Uri("/source.txt", UriKind.Relative),
            new Uri("/dest.txt", UriKind.Relative),
            parameters);

        // Assert
        Assert.AreEqual(204, result.StatusCode);
    }
}

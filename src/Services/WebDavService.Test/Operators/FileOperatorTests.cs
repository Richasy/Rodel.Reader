// Copyright (c) Richasy. All rights reserved.

using System.Net;
using System.Text;

namespace Richasy.RodelReader.Services.WebDav.Test.Operators;

[TestClass]
public class FileOperatorTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private FileOperator _operator = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        var httpClient = TestDataFactory.CreateMockHttpClient(_mockHandler);
        var dispatcher = new WebDavDispatcher(httpClient);
        _operator = new FileOperator(dispatcher);
    }

    [TestMethod]
    public async Task GetRawFileAsync_Success_ReturnsStream()
    {
        // Arrange
        var content = "Hello, WebDAV!";
        _mockHandler.When(HttpMethod.Get, "*")
            .Respond(HttpStatusCode.OK, "text/plain", content);

        // Act
        using var result = await _operator.GetRawFileAsync(new Uri("/file.txt", UriKind.Relative));

        // Assert
        Assert.IsTrue(result.IsSuccessful);
        Assert.IsNotNull(result.Stream);

        using var reader = new StreamReader(result.Stream);
        var text = await reader.ReadToEndAsync();
        Assert.AreEqual(content, text);
    }

    [TestMethod]
    public async Task GetRawFileAsync_NotFound_Returns404()
    {
        // Arrange
        _mockHandler.When(HttpMethod.Get, "*")
            .Respond(HttpStatusCode.NotFound);

        // Act
        using var result = await _operator.GetRawFileAsync(new Uri("/nonexistent.txt", UriKind.Relative));

        // Assert
        Assert.AreEqual(404, result.StatusCode);
        Assert.IsFalse(result.IsSuccessful);
    }

    [TestMethod]
    public async Task GetProcessedFileAsync_Success_ReturnsStream()
    {
        // Arrange
        var content = "File content";
        _mockHandler.When(HttpMethod.Get, "*")
            .Respond(HttpStatusCode.OK, "text/plain", content);

        // Act
        using var result = await _operator.GetProcessedFileAsync(new Uri("/file.txt", UriKind.Relative));

        // Assert
        Assert.IsTrue(result.IsSuccessful);
        Assert.IsNotNull(result.Stream);
    }

    [TestMethod]
    public async Task PutFileAsync_Success_ReturnsCreated()
    {
        // Arrange
        _mockHandler.When(HttpMethod.Put, "*")
            .Respond(HttpStatusCode.Created);

        using var content = new MemoryStream(Encoding.UTF8.GetBytes("New content"));

        // Act
        var result = await _operator.PutFileAsync(new Uri("/new-file.txt", UriKind.Relative), content);

        // Assert
        Assert.AreEqual(201, result.StatusCode);
    }

    [TestMethod]
    public async Task PutFileAsync_Overwrite_ReturnsNoContent()
    {
        // Arrange
        _mockHandler.When(HttpMethod.Put, "*")
            .Respond(HttpStatusCode.NoContent);

        using var content = new MemoryStream(Encoding.UTF8.GetBytes("Updated content"));

        // Act
        var result = await _operator.PutFileAsync(new Uri("/existing-file.txt", UriKind.Relative), content);

        // Assert
        Assert.AreEqual(204, result.StatusCode);
    }
}

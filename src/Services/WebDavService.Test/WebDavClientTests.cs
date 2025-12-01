// Copyright (c) Richasy. All rights reserved.

using System.Net;

namespace Richasy.RodelReader.Services.WebDav.Test;

[TestClass]
public class WebDavClientTests
{
    [TestMethod]
    public void Create_WithBaseAddress_ReturnsClient()
    {
        // Act
        using var client = WebDavClient.Create(new Uri("https://webdav.example.com"));

        // Assert
        Assert.IsNotNull(client);
        Assert.IsNotNull(client.Properties);
        Assert.IsNotNull(client.Resources);
        Assert.IsNotNull(client.Files);
        Assert.IsNotNull(client.Locks);
        Assert.IsNotNull(client.Search);
    }

    [TestMethod]
    public void Create_WithCredentials_ReturnsClient()
    {
        // Act
        using var client = WebDavClient.Create(
            new Uri("https://webdav.example.com"),
            "user",
            "password");

        // Assert
        Assert.IsNotNull(client);
    }

    [TestMethod]
    public void Constructor_WithOptions_ConfiguresClient()
    {
        // Arrange
        var options = new WebDavClientOptions
        {
            BaseAddress = new Uri("https://webdav.example.com"),
            Timeout = TimeSpan.FromSeconds(30),
        };

        // Act
        using var client = new WebDavClient(options);

        // Assert
        Assert.IsNotNull(client);
    }

    [TestMethod]
    public void Constructor_WithNullOptions_ThrowsException()
    {
        // Act & Assert - WebDavClientOptions cannot be null
        Assert.ThrowsExactly<NullReferenceException>(() => new WebDavClient(null!));
    }

    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var client = WebDavClient.Create(new Uri("https://webdav.example.com"));

        // Act & Assert - should not throw
        client.Dispose();
        client.Dispose();
    }

    [TestMethod]
    public async Task IntegrationTest_Propfind_WithMockHandler()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("*")
            .Respond(HttpStatusCode.MultiStatus, "application/xml", TestDataFactory.PropfindResponse);

        var httpClient = TestDataFactory.CreateMockHttpClient(mockHandler);
        var options = new WebDavClientOptions
        {
            BaseAddress = new Uri(TestDataFactory.BaseAddress),
        };

        using var client = new WebDavClient(options, httpClient, null);

        // Act
        var result = await client.Properties.PropfindAsync(
            new Uri("/documents", UriKind.Relative),
            new PropfindParameters
            {
                RequestType = PropfindRequestType.AllProperties,
            });

        // Assert
        Assert.AreEqual(207, result.StatusCode);
        Assert.IsTrue(result.Resources.Count > 0);
    }
}

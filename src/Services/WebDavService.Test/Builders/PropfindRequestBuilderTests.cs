// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav.Test.Builders;

[TestClass]
public class PropfindRequestBuilderTests
{
    [TestMethod]
    public void BuildRequest_AllProperties_ContainsPropfindElement()
    {
        // Act
        var result = PropfindRequestBuilder.BuildRequest(PropfindRequestType.AllProperties, null, null);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<D:propfind", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("<D:allprop", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildRequest_NamedProperties_ContainsPropertyElements()
    {
        // Arrange
        var properties = new[]
        {
            new WebDavProperty("displayname", WebDavConstants.DavNamespace),
            new WebDavProperty("getcontentlength", WebDavConstants.DavNamespace),
        };

        // Act
        var result = PropfindRequestBuilder.BuildRequest(PropfindRequestType.NamedProperties, properties, null);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<D:prop>", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("displayname", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("getcontentlength", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildRequest_CustomNamespace_IncludesNamespaceDeclaration()
    {
        // Arrange
        var properties = new[]
        {
            new WebDavProperty("custom", "urn:custom:ns"),
        };
        var namespaces = new[]
        {
            new NamespaceAttribute("C", "urn:custom:ns"),
        };

        // Act
        var result = PropfindRequestBuilder.BuildRequest(PropfindRequestType.NamedProperties, properties, namespaces);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("xmlns:C", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("urn:custom:ns", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildRequest_AllPropertiesImplied_ReturnsEmptyPropfind()
    {
        // Act
        var result = PropfindRequestBuilder.BuildRequest(PropfindRequestType.AllPropertiesImplied, null, null);

        // Assert
        // AllPropertiesImplied returns a propfind element without inner elements
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("propfind", StringComparison.Ordinal));
    }
}

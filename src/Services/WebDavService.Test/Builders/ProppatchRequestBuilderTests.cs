// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav.Test.Builders;

[TestClass]
public class ProppatchRequestBuilderTests
{
    [TestMethod]
    public void BuildRequestBody_SetProperty_ContainsSetElement()
    {
        // Arrange
        var propertiesToSet = new[]
        {
            new WebDavProperty("displayname", WebDavConstants.DavNamespace, "New Name"),
        };

        // Act
        var result = ProppatchRequestBuilder.BuildRequestBody(propertiesToSet, null, null);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<D:set>", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("displayname", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("New Name", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildRequestBody_RemoveProperty_ContainsRemoveElement()
    {
        // Arrange
        var propertiesToRemove = new[]
        {
            new WebDavProperty("oldprop", WebDavConstants.DavNamespace),
        };

        // Act
        var result = ProppatchRequestBuilder.BuildRequestBody(null, propertiesToRemove, null);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<D:remove>", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("oldprop", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildRequestBody_SetAndRemove_ContainsBothElements()
    {
        // Arrange
        var propertiesToSet = new[]
        {
            new WebDavProperty("newprop", WebDavConstants.DavNamespace, "value"),
        };
        var propertiesToRemove = new[]
        {
            new WebDavProperty("oldprop", WebDavConstants.DavNamespace),
        };

        // Act
        var result = ProppatchRequestBuilder.BuildRequestBody(propertiesToSet, propertiesToRemove, null);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<D:set>", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("<D:remove>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildRequestBody_EmptyProperties_ContainsPropertyupdateElement()
    {
        // Act
        var result = ProppatchRequestBuilder.BuildRequestBody(null, null, null);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("propertyupdate", StringComparison.OrdinalIgnoreCase));
    }
}

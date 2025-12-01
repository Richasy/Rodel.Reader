// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav.Test.Helpers;

[TestClass]
public class DepthHeaderHelperTests
{
    [TestMethod]
    public void GetValueForPropfind_ResourceOnly_ReturnsZero()
    {
        // Act
        var result = DepthHeaderHelper.GetValueForPropfind(ApplyTo.Propfind.ResourceOnly);

        // Assert
        Assert.AreEqual("0", result);
    }

    [TestMethod]
    public void GetValueForPropfind_ResourceAndChildren_ReturnsOne()
    {
        // Act
        var result = DepthHeaderHelper.GetValueForPropfind(ApplyTo.Propfind.ResourceAndChildren);

        // Assert
        Assert.AreEqual("1", result);
    }

    [TestMethod]
    public void GetValueForPropfind_ResourceAndAllDescendants_ReturnsInfinity()
    {
        // Act
        var result = DepthHeaderHelper.GetValueForPropfind(ApplyTo.Propfind.ResourceAndAllDescendants);

        // Assert
        Assert.AreEqual("infinity", result);
    }

    [TestMethod]
    public void GetValueForCopy_ResourceOnly_ReturnsZero()
    {
        // Act
        var result = DepthHeaderHelper.GetValueForCopy(ApplyTo.Copy.ResourceOnly);

        // Assert
        Assert.AreEqual("0", result);
    }

    [TestMethod]
    public void GetValueForCopy_ResourceAndAncestors_ReturnsInfinity()
    {
        // Act
        var result = DepthHeaderHelper.GetValueForCopy(ApplyTo.Copy.ResourceAndAncestors);

        // Assert
        Assert.AreEqual("infinity", result);
    }

    [TestMethod]
    public void GetValueForLock_ResourceOnly_ReturnsZero()
    {
        // Act
        var result = DepthHeaderHelper.GetValueForLock(ApplyTo.Lock.ResourceOnly);

        // Assert
        Assert.AreEqual("0", result);
    }

    [TestMethod]
    public void GetValueForLock_ResourceAndAncestors_ReturnsInfinity()
    {
        // Act
        var result = DepthHeaderHelper.GetValueForLock(ApplyTo.Lock.ResourceAndAncestors);

        // Assert
        Assert.AreEqual("infinity", result);
    }
}

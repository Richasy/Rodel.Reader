// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav.Test.Builders;

[TestClass]
public class LockRequestBuilderTests
{
    [TestMethod]
    public void BuildRequestBody_ExclusiveLock_ContainsExclusiveElement()
    {
        // Arrange
        var parameters = new LockParameters { LockScope = LockScope.Exclusive };

        // Act
        var result = LockRequestBuilder.BuildRequestBody(parameters);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<D:exclusive", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildRequestBody_SharedLock_ContainsSharedElement()
    {
        // Arrange
        var parameters = new LockParameters { LockScope = LockScope.Shared };

        // Act
        var result = LockRequestBuilder.BuildRequestBody(parameters);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<D:shared", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildRequestBody_WithOwner_ContainsOwnerElement()
    {
        // Arrange
        var parameters = new LockParameters
        {
            LockScope = LockScope.Exclusive,
            Owner = new PrincipalLockOwner("John Doe"),
        };

        // Act
        var result = LockRequestBuilder.BuildRequestBody(parameters);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<D:owner>", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("John Doe", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildRequestBody_WithUriOwner_ContainsHrefElement()
    {
        // Arrange
        var parameters = new LockParameters
        {
            LockScope = LockScope.Exclusive,
            Owner = new UriLockOwner(new Uri("mailto:user@example.com")),
        };

        // Act
        var result = LockRequestBuilder.BuildRequestBody(parameters);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<D:href>", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("mailto:user@example.com", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildRequestBody_ContainsWriteLockType()
    {
        // Arrange
        var parameters = new LockParameters { LockScope = LockScope.Exclusive };

        // Act
        var result = LockRequestBuilder.BuildRequestBody(parameters);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<D:write", StringComparison.Ordinal));
    }
}

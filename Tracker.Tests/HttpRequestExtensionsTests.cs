using Microsoft.AspNetCore.Http;
using Npgsql.EFCore.Tracker.AspNet.Utils;

namespace Tracker.Tests;

public class HttpRequestExtensionsTests
{
    [Fact]
    public void GetEncodedPath_WhenPathBaseAndPathHaveValues_ReturnsCombinedPath()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.PathBase = "/api";
        request.Path = "/v1/users";

        // Act
        var result = request.GetEncodedPath();

        // Assert
        Assert.Equal("/api/v1/users", result);
    }

    [Fact]
    public void GetEncodedPath_WhenOnlyPathBaseHasValue_ReturnsPathBase()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.PathBase = "/api";
        request.Path = PathString.Empty;

        // Act
        var result = request.GetEncodedPath();

        // Assert
        Assert.Equal("/api", result);
    }

    [Fact]
    public void GetEncodedPath_WhenOnlyPathHasValue_ReturnsPath()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.PathBase = PathString.Empty;
        request.Path = "/users";

        // Act
        var result = request.GetEncodedPath();

        // Assert
        Assert.Equal("/users", result);
    }

    [Fact]
    public void GetEncodedPath_WhenBothPathBaseAndPathAreEmpty_ReturnsSlash()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.PathBase = PathString.Empty;
        request.Path = PathString.Empty;

        // Act
        var result = request.GetEncodedPath();

        // Assert
        Assert.Equal("/", result);
    }

    [Fact]
    public void GetEncodedPath_WhenPathBaseHasValueAndPathIsRoot_ReturnsPathBaseWithSlash()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.PathBase = "/api";
        request.Path = "/";

        // Act
        var result = request.GetEncodedPath();

        // Assert
        Assert.Equal("/api/", result);
    }

    [Fact]
    public void GetEncodedPath_WhenPathBaseIsRootAndPathHasValue_ReturnsPathWithLeadingSlash()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.PathBase = "/";
        request.Path = "/users";

        // Act
        var result = request.GetEncodedPath();

        // Assert
        Assert.Equal("/users", result);
    }

    [Fact]
    public void GetEncodedPath_WhenBothAreRoot_ReturnsSingleSlash()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.PathBase = "/";
        request.Path = "/";

        // Act
        var result = request.GetEncodedPath();

        // Assert
        Assert.Equal("/", result);
    }

    [Fact]
    public void GetEncodedPath_WithComplexPaths_ReturnsProperlyCombinedPath()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.PathBase = "/api/v2";
        request.Path = "/products/123/details";

        // Act
        var result = request.GetEncodedPath();

        // Assert
        Assert.Equal("/api/v2/products/123/details", result);
    }

    [Fact]
    public void GetEncodedPath_WhenPathBaseHasTrailingSlashAndPathHasLeadingSlash_HandlesCorrectly()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.PathBase = "/api/";
        request.Path = "/users";

        // Act
        var result = request.GetEncodedPath();

        // Assert
        Assert.Equal("/api/users", result);
    }

    [Fact]
    public void GetEncodedPath_WhenPathBaseEmptyAndPathRoot_ReturnsSlash()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.PathBase = PathString.Empty;
        request.Path = "/";

        // Act
        var result = request.GetEncodedPath();

        // Assert
        Assert.Equal("/", result);
    }

    [Fact]
    public void GetEncodedPath_WhenPathBaseRootAndPathEmpty_ReturnsSlash()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.PathBase = "/";
        request.Path = PathString.Empty;

        // Act
        var result = request.GetEncodedPath();

        // Assert
        Assert.Equal("/", result);
    }
}
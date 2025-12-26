using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using System.Security.Claims;
using Tracker.AspNet.Models;
using Tracker.AspNet.Services;
using Tracker.AspNet.Services.Contracts;

namespace Tracker.AspNet.Tests.ServicesTests;

public class TrackerEndpointFilterTests
{
    private readonly Mock<IRequestHandler> _mockService;
    private readonly Mock<IRequestFilter> _mockFilter;
    private readonly ImmutableGlobalOptions _mockOptions;
    private readonly TrackerEndpointFilter _filter;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly EndpointFilterInvocationContext _filterContext;
    private readonly Mock<EndpointFilterDelegate> _mockNext;

    public TrackerEndpointFilterTests()
    {
        _mockService = new Mock<IRequestHandler>();
        _mockFilter = new Mock<IRequestFilter>();
        _mockOptions = new ImmutableGlobalOptions();
        _filter = new TrackerEndpointFilter(_mockService.Object, _mockFilter.Object, _mockOptions);

        _mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockResponse = new Mock<HttpResponse>();
        var mockConnection = new Mock<ConnectionInfo>();
        var mockFeatures = new Mock<IFeatureCollection>();
        var mockItems = new Dictionary<object, object?>();
        var mockUser = new Mock<ClaimsPrincipal>();

        _mockHttpContext.Setup(x => x.Request).Returns(mockRequest.Object);
        _mockHttpContext.Setup(x => x.Response).Returns(mockResponse.Object);
        _mockHttpContext.Setup(x => x.Connection).Returns(mockConnection.Object);
        _mockHttpContext.Setup(x => x.Features).Returns(mockFeatures.Object);
        _mockHttpContext.Setup(x => x.Items).Returns(mockItems);
        _mockHttpContext.Setup(x => x.User).Returns(mockUser.Object);

        var arguments = new object[] { _mockHttpContext.Object };
        _filterContext = new DefaultEndpointFilterInvocationContext(_mockHttpContext.Object, arguments);

        _mockNext = new Mock<EndpointFilterDelegate>();
    }

    [Fact]
    public async Task InvokeAsync_RequestInvalid_ReturnsNextResult()
    {
        // Arrange
        var expectedResult = Results.Ok("success");
        _mockFilter.Setup(x => x.ValidRequest(_mockHttpContext.Object, _mockOptions))
            .Returns(false);
        _mockNext.Setup(x => x(It.IsAny<EndpointFilterInvocationContext>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _filter.InvokeAsync(_filterContext, _mockNext.Object);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockFilter.Verify(x => x.ValidRequest(_mockHttpContext.Object, _mockOptions), Times.Once);
        _mockService.Verify(x => x.HandleRequest(It.IsAny<HttpContext>(), It.IsAny<ImmutableGlobalOptions>(), default), Times.Never);
        _mockNext.Verify(x => x(_filterContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_RequestValid_IsNotModifiedFalse_ReturnsNextResult()
    {
        // Arrange
        var expectedResult = Results.Ok("success");
        _mockFilter.Setup(x => x.ValidRequest(_mockHttpContext.Object, _mockOptions))
            .Returns(true);
        _mockService.Setup(x => x.HandleRequest(_mockHttpContext.Object, _mockOptions, default))
            .ReturnsAsync(false);
        _mockNext.Setup(x => x(It.IsAny<EndpointFilterInvocationContext>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _filter.InvokeAsync(_filterContext, _mockNext.Object);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockFilter.Verify(x => x.ValidRequest(_mockHttpContext.Object, _mockOptions), Times.Once);
        _mockService.Verify(x => x.HandleRequest(_mockHttpContext.Object, _mockOptions, default), Times.Once);
        _mockNext.Verify(x => x(_filterContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_RequestValid_IsNotModifiedTrue_Returns304StatusCode()
    {
        // Arrange
        _mockFilter.Setup(x => x.ValidRequest(_mockHttpContext.Object, _mockOptions))
            .Returns(true);
        _mockService.Setup(x => x.HandleRequest(_mockHttpContext.Object, _mockOptions, default))
            .ReturnsAsync(true);
        _mockNext.Setup(x => x(It.IsAny<EndpointFilterInvocationContext>()))
            .ReturnsAsync(Results.Ok("should not be called"));

        // Act
        var result = await _filter.InvokeAsync(_filterContext, _mockNext.Object);
        var typedResult = result as IStatusCodeHttpResult;

        // Assert
        Assert.NotNull(typedResult);
        Assert.Equal(StatusCodes.Status304NotModified, typedResult.StatusCode);
        _mockFilter.Verify(x => x.ValidRequest(_mockHttpContext.Object, _mockOptions), Times.Once);
        _mockService.Verify(x => x.HandleRequest(_mockHttpContext.Object, _mockOptions, default), Times.Once);
        _mockNext.Verify(x => x(It.IsAny<EndpointFilterInvocationContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ServiceThrowsException_ExceptionPropagates()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Service error");
        _mockFilter.Setup(x => x.ValidRequest(_mockHttpContext.Object, _mockOptions))
            .Returns(true);
        _mockService.Setup(x => x.HandleRequest(_mockHttpContext.Object, _mockOptions, default))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _filter.InvokeAsync(_filterContext, _mockNext.Object));

        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task InvokeAsync_FilterThrowsException_ExceptionPropagates()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Filter error");
        _mockFilter.Setup(x => x.ValidRequest(_mockHttpContext.Object, _mockOptions))
            .Throws(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _filter.InvokeAsync(_filterContext, _mockNext.Object));

        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task InvokeAsync_NullFilterContext_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(async () =>
            await _filter.InvokeAsync(null!, _mockNext.Object));
    }

    [Fact]
    public async Task InvokeAsync_NullNext_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(async () =>
            await _filter.InvokeAsync(_filterContext, null!));
    }

    [Fact]
    public async Task InvokeAsync_NextReturnsNull_ReturnsNull()
    {
        // Arrange
        _mockFilter.Setup(x => x.ValidRequest(_mockHttpContext.Object, _mockOptions))
            .Returns(false);
        _mockNext.Setup(x => x(It.IsAny<EndpointFilterInvocationContext>()))
            .ReturnsAsync((object?)null);

        // Act
        var result = await _filter.InvokeAsync(_filterContext, _mockNext.Object);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task InvokeAsync_StatusCodeResult_VerifiesStatusCode()
    {
        // Arrange
        _mockFilter.Setup(x => x.ValidRequest(_mockHttpContext.Object, _mockOptions))
            .Returns(true);
        _mockService.Setup(x => x.HandleRequest(_mockHttpContext.Object, _mockOptions, default))
            .ReturnsAsync(true);

        // Act
        var result = await _filter.InvokeAsync(_filterContext, _mockNext.Object);

        // Assert - Test that we can cast to IStatusCodeHttpResult
        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status304NotModified, statusResult.StatusCode);
    }
}
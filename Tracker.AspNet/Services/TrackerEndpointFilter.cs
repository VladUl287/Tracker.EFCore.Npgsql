using Microsoft.AspNetCore.Http;
using Tracker.AspNet.Models;
using Tracker.AspNet.Services.Contracts;

namespace Tracker.AspNet.Services;

public sealed class TrackerEndpointFilter : IEndpointFilter
{
    private readonly IETagService _eTagService;
    private readonly IRequestFilter _requestFilter;
    private readonly ImmutableGlobalOptions _options;

    public TrackerEndpointFilter(IETagService eTagService, IRequestFilter requestFilter, ImmutableGlobalOptions options)
    {
        ArgumentNullException.ThrowIfNull(eTagService, nameof(eTagService));
        ArgumentNullException.ThrowIfNull(requestFilter, nameof(requestFilter));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        _eTagService = eTagService;
        _requestFilter = requestFilter;
        _options = options;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpCtx = context.HttpContext;
        var token = httpCtx.RequestAborted;

        var shouldProcessRequest = _requestFilter.ShouldProcessRequest(httpCtx, _options);
        if (!shouldProcessRequest)
            return await next(context);

        if (token.IsCancellationRequested)
            return Results.BadRequest();

        var shouldReturnNotModified = await _eTagService.TrySetETagAsync(httpCtx, _options, token);
        if (!shouldReturnNotModified)
            return await next(context);

        return Results.StatusCode(StatusCodes.Status304NotModified);
    }
}

using Microsoft.AspNetCore.Http;
using Tracker.AspNet.Models;
using Tracker.AspNet.Services.Contracts;

namespace Tracker.AspNet.Middlewares;

public sealed class TrackerMiddleware(
    RequestDelegate next, IRequestFilter filter, IRequestHandler service,
    ImmutableGlobalOptions opts)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        if (filter.ValidRequest(ctx, opts) && await service.HandleRequest(ctx, opts))
            return;

        await next(ctx);
    }
}
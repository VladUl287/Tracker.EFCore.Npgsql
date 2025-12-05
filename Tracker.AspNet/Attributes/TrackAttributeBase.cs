using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using Tracker.AspNet.Models;
using Tracker.AspNet.Services.Contracts;

namespace Tracker.AspNet.Attributes;

public abstract class TrackAttributeBase(string[]? tables, string? sourceId, string? cacheControl) : Attribute, IAsyncActionFilter
{
    protected readonly ImmutableArray<string> _tables = tables?.ToImmutableArray() ?? [];
    protected readonly string? _sourceId = sourceId;
    protected readonly string? _cacheControl = cacheControl;

    public async Task OnActionExecutionAsync(ActionExecutingContext execContext, ActionExecutionDelegate next)
    {
        var options = GetOrSetOptions(execContext);

        var httpCtx = execContext.HttpContext;
        var reqServices = httpCtx.RequestServices;

        var requestFilter = reqServices.GetRequiredService<IRequestFilter>();
        var shouldProcessRequest = requestFilter.ShouldProcessRequest(httpCtx, options);
        if (!shouldProcessRequest)
        {
            await next();
            return;
        }

        var cancelToken = httpCtx.RequestAborted;
        if (cancelToken.IsCancellationRequested)
            return;

        var etagService = reqServices.GetRequiredService<IETagService>();
        var shouldReturnNotModified = await etagService.TrySetETagAsync(httpCtx, options, cancelToken);
        if (!shouldReturnNotModified)
        {
            await next();
            return;
        }
    }

    protected abstract ImmutableGlobalOptions GetOrSetOptions(ActionExecutingContext execContext);
}

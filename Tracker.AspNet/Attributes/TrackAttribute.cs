using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Tracker.AspNet.Models;
using Tracker.AspNet.Services.Contracts;

namespace Tracker.AspNet.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class TrackAttribute : Attribute, IAsyncActionFilter
{
    private static readonly ConcurrentDictionary<string, ImmutableGlobalOptions> _optionsCache = new();
    private readonly ImmutableArray<string> _tables = [];

    public TrackAttribute(params string[] tables)
    {
        ArgumentNullException.ThrowIfNull(tables, nameof(tables));
        _tables = [.. tables];
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext execContext, ActionExecutionDelegate next)
    {
        var options = GetOptions(execContext);

        var httpCtx = execContext.HttpContext;
        var canceltoken = execContext.HttpContext.RequestAborted;

        var requestFilter = httpCtx.RequestServices.GetRequiredService<IRequestFilter>();
        var shouldProcessRequest = requestFilter.ShouldProcessRequest(httpCtx, options);
        if (!shouldProcessRequest)
        {
            await next();
            return;
        }

        if (canceltoken.IsCancellationRequested)
            return;

        var etagService = execContext.HttpContext.RequestServices.GetRequiredService<IETagService>();
        var shouldReturnNotModified = await etagService.TrySetETagAsync(httpCtx, options, canceltoken);
        if (!shouldReturnNotModified)
        {
            await next();
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImmutableGlobalOptions GetOptions(ActionExecutingContext execContext)
    {
        var actionId = execContext.ActionDescriptor.Id;
        var httpCtx = execContext.HttpContext;
        return _optionsCache.GetOrAdd(actionId,
            (key, state) =>
            {
                var baseOptions = state.httpCtx.RequestServices.GetRequiredService<ImmutableGlobalOptions>();
                return baseOptions with { Tables = _tables };
            },
            (httpCtx, _tables));
    }
}
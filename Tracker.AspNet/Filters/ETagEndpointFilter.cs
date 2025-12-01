using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Tracker.AspNet.Services.Contracts;

namespace Tracker.AspNet.Filters;

public sealed class ETagEndpointFilter : IEndpointFilter
{
    public ETagEndpointFilter()
    { }

    public ETagEndpointFilter(string[] tables)
    {
        ArgumentNullException.ThrowIfNull(tables, nameof(tables));
        Tables = tables;
    }

    public string[] Tables { get; } = [];
    public bool IsGlobalTrack => Tables is null or { Length: 0 };

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (IsGetMethod(context.HttpContext))
        {
            var etagService = context.HttpContext.RequestServices.GetRequiredService<IETagService>();
            var token = context.HttpContext.RequestAborted;

            if (IsGlobalTrack)
            {
                if (await etagService.TrySetETagAsync(context.HttpContext, token))
                    return Results.StatusCode(StatusCodes.Status304NotModified);
            }
            else
            {
                if (await etagService.TrySetETagAsync(context.HttpContext, Tables, token))
                    return Results.StatusCode(StatusCodes.Status304NotModified);
            }
        }

        return await next(context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsGetMethod(HttpContext context) => context.Request.Method == HttpMethod.Get.Method;
}

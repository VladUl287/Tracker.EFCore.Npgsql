using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Tracker.AspNet.Logging;
using Tracker.AspNet.Models;
using Tracker.AspNet.Services.Contracts;

namespace Tracker.AspNet.Services;

public sealed class DefaultRequestFilter(ILogger<DefaultRequestFilter> logger) : IRequestFilter
{
    public bool RequestValid(HttpContext ctx, ImmutableGlobalOptions options)
    {
        logger.LogFilterStarted(ctx.TraceIdentifier, ctx.Request.Path);

        if (!HttpMethods.IsGet(ctx.Request.Method))
        {
            logger.LogNotGetRequest(ctx.Request.Method, ctx.TraceIdentifier);
            return false;
        }

        if (ctx.Response.Headers.ETag.Count > 0)
        {
            logger.LogEtagHeaderPresented(ctx.TraceIdentifier);
            return false;
        }

        if (AnyInvalidCacheControl(ctx.Request.Headers.CacheControl, out var reqDirective))
        {
            logger.LogRequestNotValidCacheControlDirective(reqDirective, ctx.TraceIdentifier);
            return false;
        }

        if (AnyInvalidCacheControl(ctx.Response.Headers.CacheControl, out var resDirective))
        {
            logger.LogResponseNotValidCacheControlDirective(resDirective, ctx.TraceIdentifier);
            return false;
        }

        if (!options.Filter(ctx))
        {
            logger.LogFilterRejected(ctx.TraceIdentifier);
            return false;
        }

        logger.LogContextFilterFinished(ctx.TraceIdentifier);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AnyInvalidCacheControl(StringValues cacheControlHeaders, [NotNullWhen(true)] out string? directive)
    {
        directive = null;

        if (cacheControlHeaders.Count == 0)
            return false;

        const string IMMUTABLE = "immutable";
        const string NO_STORE = "no-store";
        foreach (var header in cacheControlHeaders)
        {
            if (header is null)
                continue;

            if (header.Contains(IMMUTABLE, StringComparison.OrdinalIgnoreCase))
            {
                directive = IMMUTABLE;
                return true;
            }

            if (header.Contains(NO_STORE, StringComparison.OrdinalIgnoreCase))
            {
                directive = NO_STORE;
                return true;
            }
        }

        return false;
    }
}

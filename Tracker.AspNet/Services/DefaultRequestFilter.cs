using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Tracker.AspNet.Logging;
using Tracker.AspNet.Models;
using Tracker.AspNet.Services.Contracts;

namespace Tracker.AspNet.Services;

public sealed class DefaultRequestFilter(ILogger<DefaultRequestFilter> logger) : IRequestFilter
{
    public bool ValidRequest(HttpContext ctx, ImmutableGlobalOptions opts)
    {
        var traceId = new TraceId(ctx);

        logger.LogFilterStarted(traceId, ctx.Request.Path);

        if (!HttpMethods.IsGet(ctx.Request.Method))
        {
            logger.LogNotGetRequest(ctx.Request.Method, traceId);
            return false;
        }

        if (ctx.Response.Headers.ETag.Count > 0)
        {
            logger.LogEtagHeaderPresented(traceId);
            return false;
        }

        if (AnyInvalidDirective(ctx.Request.Headers.CacheControl, opts.InvalidRequestDirectives, out var reqDirective))
        {
            logger.LogRequestNotValidCacheControlDirective(reqDirective, traceId);
            return false;
        }

        if (AnyInvalidDirective(ctx.Response.Headers.CacheControl, opts.InvalidResponseDirectives, out var resDirective))
        {
            logger.LogResponseNotValidCacheControlDirective(resDirective, traceId);
            return false;
        }

        if (!opts.Filter(ctx))
        {
            logger.LogFilterRejected(traceId);
            return false;
        }

        logger.LogContextFilterFinished(traceId);
        return true;
    }

    internal static bool AnyInvalidDirective(StringValues headers, ImmutableArray<string> invalidDirectives, [NotNullWhen(true)] out string? directive)
    {
        directive = null;

        if (headers.Count == 0)
            return false;

        foreach (var header in headers)
        {
            if (header is null)
                continue;

            foreach (var invalidDirective in invalidDirectives)
            {
                if (header.Contains(invalidDirective, StringComparison.OrdinalIgnoreCase))
                {
                    directive = invalidDirective;
                    return true;
                }
            }
        }

        return false;
    }
}

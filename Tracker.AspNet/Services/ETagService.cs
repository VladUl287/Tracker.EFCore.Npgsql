using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tracker.AspNet.Logging;
using Tracker.AspNet.Services.Contracts;
using Tracker.Core.Extensions;

namespace Tracker.AspNet.Services;

public class ETagService<TContext>(
    IETagGenerator etagGenerator, ILogger<ETagService<TContext>> logger) : IETagService where TContext : DbContext
{
    public async Task<bool> TrySetETagAsync(HttpContext context, string[] tables, CancellationToken token = default)
    {
        if (context.Response.Headers.ETag.Count != 0)
        {
            logger.LogETagAlreadyExists();
            return false;
        }

        var dbContext = context.RequestServices.GetService<TContext>();
        if (dbContext is null)
        {
            logger.LogDbContextNotFound(typeof(TContext).Name);
            return false;
        }

        var timestamps = new List<DateTimeOffset>(tables.Length);
        foreach (var table in tables)
        {
            var lastTimestamp = await dbContext.GetLastTimestamp(table, token);

            if (string.IsNullOrEmpty(lastTimestamp))
            {
                logger.LogLastTimestampNotFound();
                return false;
            }

            timestamps.Add(DateTimeOffset.Parse(lastTimestamp));
        }

        var etag = etagGenerator.GenerateETag([.. timestamps]);
        if (context.Request.Headers.IfNoneMatch == etag)
        {
            logger.LogNotModified(etag);
            return true;
        }

        logger.LogETagAdded(etag);
        context.Response.Headers.ETag = etag;

        return false;
    }

    public async Task<bool> TrySetETagAsync(HttpContext context, CancellationToken token = default)
    {
        if (context.Response.Headers.ETag.Count != 0)
        {
            logger.LogETagAlreadyExists();
            return false;
        }

        var dbContext = context.RequestServices.GetService<TContext>();
        if (dbContext is null)
        {
            logger.LogDbContextNotFound(typeof(TContext).Name);
            return false;
        }

        var xact = await dbContext.GetLastCommittedXact(token);
        if (xact is null)
        {
            logger.LogLastTimestampNotFound();
            return false;
        }

        var etag = etagGenerator.GenerateETag(xact.Value);
        if (context.Request.Headers.IfNoneMatch == etag)
        {
            logger.LogNotModified(etag);
            return true;
        }

        logger.LogETagAdded(etag);
        context.Response.Headers.ETag = etag;

        return false;
    }
}

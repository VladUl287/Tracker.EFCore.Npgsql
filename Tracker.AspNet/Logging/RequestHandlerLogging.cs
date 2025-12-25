using Microsoft.Extensions.Logging;

namespace Tracker.AspNet.Logging;

public static partial class RequestHandlerLogging
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Request handler started. TraceId: {TraceId}. Path - {Path}")]
    public static partial void LogRequestHandleStarted(this ILogger logger, TraceId traceId, string path);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Request handler finished - TraceId: {TraceId}")]
    public static partial void LogRequestHandleFinished(this ILogger logger, TraceId traceId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Resource not modified. TraceId: '{TraceId}'. ETag: {ETag}")]
    public static partial void LogNotModified(this ILogger logger, TraceId traceId, string etag);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Data not modified. Return 304 status code. TraceId: '{TraceId}'")]
    public static partial void LogNotModified(this ILogger logger, TraceId traceId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ETag added to response: {ETag}. TraceId: '{TraceId}'")]
    public static partial void LogETagAdded(this ILogger logger, string etag, TraceId traceId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Source operations provider resolved for request: TraceId - {TraceId}, SourceId - {SourceId}")]
    public static partial void LogSourceProviderResolved(this ILogger logger, TraceId traceId, string sourceId);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Specified source provider '{SourceId}' is not registered. Falling back to default providers. TraceId: {TraceId}")]
    public static partial void LogSourceProviderNotRegistered(this ILogger logger, string sourceId, TraceId traceId);
}

using Microsoft.Extensions.Logging;

namespace Tracker.AspNet.Logging;

public static partial class RequestFilterLogging
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Context filter started. TraceId - '{TraceId}'. Path - '{path}'")]
    public static partial void LogFilterStarted(this ILogger logger, string traceId, string path);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Context filter not passed: Method '{Method}' must be GET. TraceId - '{TraceId}'")]
    public static partial void LogNotGetRequest(this ILogger logger, string method, string traceId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Context filter not passed: Already contains ETag. TraceId - '{TraceId}'")]
    public static partial void LogEtagHeaderPresented(this ILogger logger, string traceId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Context filter not passed: Request have invalid cache control directive '{Directive}'. TraceId - {TraceId}")]
    public static partial void LogRequestNotValidCacheControlDirective(this ILogger logger, string directive, string traceId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Context filter not passed: Response have invalid cache control directive '{Directive}'. TraceId - {TraceId}")]
    public static partial void LogResponseNotValidCacheControlDirective(this ILogger logger, string directive, string traceId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Context filter not passed: Custom filter rejected. TraceId - {TraceId}")]
    public static partial void LogFilterRejected(this ILogger logger, string traceId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Context filter finished. TraceId - '{TraceId}'")]
    public static partial void LogContextFilterFinished(this ILogger logger, string traceId);
}

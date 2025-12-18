using Microsoft.AspNetCore.Http;

namespace Tracker.AspNet;

/// <summary>
/// Wraps <see cref="HttpContext.TraceIdentifier"/> to defer string allocation until needed.
/// Used when logging is disabled to avoid unnecessary string creation.
/// </summary>
public readonly struct TraceId(HttpContext httpContext)
{
    public override string ToString() => httpContext.TraceIdentifier;
}

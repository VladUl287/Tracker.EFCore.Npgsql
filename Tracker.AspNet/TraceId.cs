using Microsoft.AspNetCore.Http;

namespace Tracker.AspNet;

public readonly struct TraceId(HttpContext httpContext)
{
    public override string ToString() => httpContext.TraceIdentifier;
}

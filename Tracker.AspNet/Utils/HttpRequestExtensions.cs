using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;

namespace Tracker.AspNet.Utils;

public static class HttpRequestExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetEncodedPath(this HttpRequest request)
    {
        PathString pathBase = request.PathBase, path = request.Path;
        return (pathBase.HasValue || path.HasValue) ? (pathBase + path).ToString() : "/";
    }
}

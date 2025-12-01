using Microsoft.AspNetCore.Http;

namespace Tracker.AspNet.Models;

public sealed class MiddlewareOptions
{
    public Func<HttpContext, bool> Filter { get; set; } = (_) => true;
    
    public string[] Tables { get; set; } = [];
    public Type[] Entities { get; set; } = [];

    public Func<HttpContext, string> Suffix { get; set; } = (_) => string.Empty;
}

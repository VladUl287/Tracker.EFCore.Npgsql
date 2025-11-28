using Microsoft.AspNetCore.Http;
using Tracker.AspNet.Services.Contracts;
using Tracker.AspNet.Utils;

namespace Tracker.AspNet.Services;

public class DefaultPathResolver : IPathResolver
{
    public virtual string ResolvePath(HttpContext context)
    {
        return context.Request.GetEncodedPath();
    }
}

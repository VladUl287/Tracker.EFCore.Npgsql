using Microsoft.AspNetCore.Http;

namespace Tracker.AspNet.Services.Contracts;

public interface IPathResolver
{
    string ResolvePath(HttpContext context);
}

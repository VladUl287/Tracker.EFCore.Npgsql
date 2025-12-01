using Microsoft.AspNetCore.Http;

namespace Tracker.AspNet.Services.Contracts;

public interface IETagService
{
    Task<bool> TrySetETagAsync(HttpContext context, string[] tables, CancellationToken token = default);
}

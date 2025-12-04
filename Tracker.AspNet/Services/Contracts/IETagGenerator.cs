using Tracker.AspNet.Models;

namespace Tracker.AspNet.Services.Contracts;

public interface IETagGenerator
{
    string GenerateETag(DateTimeOffset timestamp);
    string GenerateETag(params DateTimeOffset[] timestamps);
    string GenerateETag(uint xact);
}

using Tracker.AspNet.Services.Contracts;
using Tracker.AspNet.Utils;

namespace Tracker.AspNet.Services;

public class ETagGenerator : IETagGenerator
{
    public string GenerateETag(DateTimeOffset timestamp)
    {
        return ETagUtils.GenETagTicks(timestamp);
    }

    public string GenerateETag(params DateTimeOffset[] timestamps)
    {
        return ETagUtils.GenETagTicks(timestamps);
    }
}

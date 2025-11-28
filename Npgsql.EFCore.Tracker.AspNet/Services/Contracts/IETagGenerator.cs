namespace Npgsql.EFCore.Tracker.AspNet.Services.Contracts;

public interface IETagGenerator
{
    string GenerateETagTicks(DateTimeOffset timestamp);
    string GenerateETagSHA256(DateTimeOffset timestamp);
    string GenerateETagTicks(params DateTimeOffset[] timestamps);
    string GenerateETagSHA256(params DateTimeOffset[] timestamps);
}

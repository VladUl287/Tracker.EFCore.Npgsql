namespace Tracker.AspNet.Services.Contracts;

public interface IDbOperations
{
    Task<DateTimeOffset?> GetLastTimestamp(string table, CancellationToken token);
    Task<uint?> GetLastCommittedXact(CancellationToken token);
}

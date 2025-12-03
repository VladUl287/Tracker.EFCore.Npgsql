namespace Tracker.AspNet.Services.Contracts;

public interface IDbOperations
{
    Task<DateTimeOffset?> GetLastTimestamp(string table);
    Task<uint?> GetLastCommittedXact(string table);
}

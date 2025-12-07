namespace Tracker.Core.Services.Contracts;

public interface ISourceOperations
{
    string SourceId { get; }
    Task<DateTimeOffset?> GetLastTimestamp(string key, CancellationToken token);
    Task<DateTimeOffset[]?> GetLastTimestamps(string[] keys, CancellationToken token);
    Task<DateTimeOffset?> GetLastTimestamp(CancellationToken token);
}

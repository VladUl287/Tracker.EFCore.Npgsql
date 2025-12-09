namespace Tracker.Core.Services.Contracts;

public interface ITimestampsHasher
{
    long Hash(Span<DateTimeOffset> timestamps);
}

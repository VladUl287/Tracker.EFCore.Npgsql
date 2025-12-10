namespace Tracker.AspNet.Services.Contracts;

public interface IETagService
{
    string AssemblyBuildTimeTicks { get; }
    bool EqualsTo(string ifNoneMatch, int fullLength, ulong lastTimestamp, string suffix);
    string Build(int fullLength, ulong lastTimestamp, string suffix);
}

using Tracker.Core.Services.Contracts;

namespace Tracker.Core.Services;

public sealed class Fnv1aTimstampsHasher : ITimestampsHasher
{
    private const ulong FnvOffsetBasis = 14695981039346656037UL;
    private const ulong FnvPrime = 1099511628211UL;

    public long Hash(Span<DateTimeOffset> timestamps)
    {
        ulong hash = FnvOffsetBasis;

        foreach (var timestamp in timestamps)
        {
            byte[] bytes = BitConverter.GetBytes(timestamp.Ticks);

            foreach (byte b in bytes)
            {
                hash ^= b;
                hash *= FnvPrime;
            }
        }

        return (long)hash;
    }

    public long HashLongs(DateTimeOffset[] numbers)
    {
        ulong hash = FnvOffsetBasis;

        foreach (var date in numbers)
        {
            long ticks = date.Ticks;

            for (int i = 0; i < 8; i++)
            {
                byte b = (byte)(ticks >> (i * 8));
                hash ^= b;
                hash *= FnvPrime;
            }
        }

        return (long)hash;
    }
}

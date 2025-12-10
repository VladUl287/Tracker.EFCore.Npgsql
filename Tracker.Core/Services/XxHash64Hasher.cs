using System.Buffers;
using System.Buffers.Binary;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using Tracker.Core.Services.Contracts;

namespace Tracker.Core.Services;

public sealed class XxHash64Hasher : ITimestampsHasher
{
    public ulong Hash(ReadOnlySpan<DateTimeOffset> timestamps)
    {
        const int BufferSizeThreshold = 128;

        if (BitConverter.IsLittleEndian)
        {
            Span<long> ticks = stackalloc long[timestamps.Length];
            for (int i = 0; i < timestamps.Length; i++)
                ticks[i] = timestamps[i].Ticks;

            return XxHash64.HashToUInt64(MemoryMarshal.AsBytes(ticks));
        }

        var bufferSize = timestamps.Length * sizeof(long);
        Span<byte> buffer = stackalloc byte[bufferSize];

        if (bufferSize >= BufferSizeThreshold)
        {
            var arr = ArrayPool<byte>.Shared.Rent(bufferSize);
            Span<byte> bufferArr = arr.AsSpan();

            for (int i = 0; i < timestamps.Length; i++)
                BinaryPrimitives.WriteInt64LittleEndian(
                    bufferArr.Slice(i * sizeof(long), sizeof(long)),
                    timestamps[i].Ticks);

            ArrayPool<byte>.Shared.Return(arr);
            return XxHash64.HashToUInt64(bufferArr);
        }

        for (int i = 0; i < timestamps.Length; i++)
            BinaryPrimitives.WriteInt64LittleEndian(
                buffer.Slice(i * sizeof(long), sizeof(long)),
                timestamps[i].Ticks);

        return XxHash64.HashToUInt64(buffer);
    }
}

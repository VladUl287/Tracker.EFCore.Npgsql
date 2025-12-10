using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

namespace Tracker.Benchmarks;

[MemoryDiagnoser]
public unsafe class ETagComparerBenchmark
{
    private static readonly string IncomingETag = "539007748579998180-631437120000000000-suffix";
    private static readonly long AssemblyBuildTime = 639007748579998180;
    private static readonly string AssemblyBuildTimeString = AssemblyBuildTime.ToString();
    private static readonly string Suffix = "suffix";
    private static readonly long LastTimestamp = 631437120000000000;

    [Benchmark]
    public string? Compare_Equal_AlwaysGenerate()
    {
        var incominigEtag = IncomingETag;

        var etag = $"{AssemblyBuildTime}-{LastTimestamp}-{Suffix}";
        if (etag == incominigEtag)
            return null;

        return etag;
    }

    [Benchmark]
    public string? Compare_Equal_PartialGenerate()
    {
        var incominigEtag = IncomingETag.AsSpan();

        var digitCount = AssemblyBuildTimeString.Length;
        var inAssemblyBuildTime = incominigEtag[..digitCount];

        var lastTimestampDigitCount = DigitCountLog(LastTimestamp);

        var fullLength = AssemblyBuildTimeString.Length + 2 + lastTimestampDigitCount + Suffix.Length;
        if (fullLength != incominigEtag.Length)
        {
            return BuildETag(fullLength, (AssemblyBuildTimeString, LastTimestamp, Suffix));
        }

        if (!inAssemblyBuildTime.Equals(AssemblyBuildTimeString.AsSpan(), StringComparison.Ordinal))
        {
            return BuildETag(fullLength, (AssemblyBuildTimeString, LastTimestamp, Suffix));
        }

        var start = digitCount + 1;
        var inTicks = incominigEtag.Slice(start, lastTimestampDigitCount);

        if (!FastCompareStringToLongSimd(inTicks, LastTimestamp))
        {
            return BuildETag(fullLength, (AssemblyBuildTimeString, LastTimestamp, Suffix));
        }

        start += lastTimestampDigitCount + 1;
        var inSuffix = incominigEtag[start..];

        if (!inSuffix.Equals(Suffix, StringComparison.Ordinal))
        {
            return BuildETag(fullLength, (AssemblyBuildTimeString, LastTimestamp, Suffix));
        }

        return null;
    }

    [Benchmark]
    public string? Compare_Equal_PartialGenerate_BuildETagV2()
    {
        var incominigEtag = IncomingETag.AsSpan();

        Span<char> ltValue = stackalloc char[18];
        LastTimestamp.TryFormat(ltValue, out var ltWritten);

        var fullLength = AssemblyBuildTimeString.Length + 2 + ltWritten + Suffix.Length;
        if (fullLength != incominigEtag.Length)
            return BuildETag(fullLength, AssemblyBuildTimeString, ltValue, Suffix);

        var rightEdge = AssemblyBuildTimeString.Length;
        var inAssemblyBuildTime = incominigEtag[..rightEdge];
        if (!inAssemblyBuildTime.Equals(AssemblyBuildTimeString.AsSpan(), StringComparison.Ordinal))
            return BuildETag(fullLength, AssemblyBuildTimeString, ltValue, Suffix);

        var inTicks = incominigEtag.Slice(++rightEdge, ltWritten);
        if (!inTicks.Equals(ltValue, StringComparison.Ordinal))
            return BuildETag(fullLength, AssemblyBuildTimeString, ltValue, Suffix);

        rightEdge += inTicks.Length + 1;
        var inSuffix = incominigEtag[rightEdge..];
        if (!inSuffix.Equals(Suffix, StringComparison.Ordinal))
            return BuildETag(fullLength, AssemblyBuildTimeString, ltValue, Suffix);

        return null;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BuildETag(int fullLength, (string AsBuldTime, long LastTimestamp, string Suffix) state) =>
        string.Create(fullLength, state, (chars, state) =>
        {
            var position = state.AsBuldTime.Length;
            state.AsBuldTime.AsSpan().CopyTo(chars);
            chars[position++] = '-';

            state.LastTimestamp.TryFormat(chars[position..], out var written);
            position += written;
            chars[position++] = '-';

            state.Suffix.AsSpan().CopyTo(chars[position..]);
        });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string BuildETag(int fullLength, ReadOnlySpan<char> asBuildTime, ReadOnlySpan<char> timestamp, ReadOnlySpan<char> suffix)
    {
        Span<char> chars = stackalloc char[fullLength];

        var position = asBuildTime.Length;
        asBuildTime.CopyTo(chars);
        chars[position++] = '-';

        timestamp.CopyTo(chars[position..]);

        if (suffix.Length > 0)
        {
            position += timestamp.Length;
            chars[position++] = '-';
            suffix.CopyTo(chars[position..]);
        }

        return new string(chars);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FastCompareStringToLongSimd(ReadOnlySpan<char> str, long number)
    {
        if (str.Length > 19)
            return false;

        long result = 0;
        foreach (var c in str)
        {
            if (c < '0' || c > '9') return false;
            result = result * 10 + (c - '0');
        }

        return result == number;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DigitCountLog(long n)
    {
        if (n == 0) return 1;
        return (int)Math.Floor(Math.Log10(n)) + 1;
    }
}

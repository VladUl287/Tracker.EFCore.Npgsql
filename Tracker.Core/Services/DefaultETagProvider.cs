using System.Runtime.CompilerServices;
using Tracker.Core.Extensions;
using Tracker.Core.Services.Contracts;

[assembly: InternalsVisibleTo("Tracker.Core.Tests")]

namespace Tracker.Core.Services;

/// <summary>
/// Implementation of <see cref="IETagProvider"/> that uses assembly timestamp, entity timestamp, and an optional suffix
/// for versioning.
/// </summary>
/// <remarks>
/// The ETag format is: {assemblyTimestamp}-{entityTimestamp}[-{suffix}]
/// where:<br/>
/// - assemblyTimestamp is obtained from <see cref="IAssemblyTimestampProvider"/><br/>
/// - entityTimestamp is the last modified timestamp of the entity<br/>
/// - suffix is an optional identifier (e.g., content hash, version identifier)<br/>
/// The suffix is optional and only included when provided.
/// </remarks>
public sealed class DefaultETagProvider(IAssemblyTimestampProvider assemblyTimestampProvider) : IETagProvider
{
    private readonly string _assemblyTimestamp = assemblyTimestampProvider.GetWriteTime().Ticks.ToString();

    /// <inheritdoc/>
    public bool Compare(string etag, ulong lastTimestamp, string suffix)
    {
        var timestampDigitCount = lastTimestamp.CountDigits();
        var expectedLength = CalculateEtagLength(timestampDigitCount, suffix.Length);

        if (expectedLength != etag.Length)
            return false;

        var etagSpan = etag.AsSpan();
        var position = _assemblyTimestamp.Length;

        var assemblyTimestampSegment = etagSpan[..position];
        if (!assemblyTimestampSegment.Equals(_assemblyTimestamp.AsSpan(), StringComparison.Ordinal))
            return false;

        if (etagSpan[position] != '-')
            return false;

        var timestampSegment = etagSpan.Slice(++position, timestampDigitCount);
        if (!timestampSegment.EqualsULong(lastTimestamp))
            return false;

        position += timestampDigitCount;

        if (position == etagSpan.Length)
            return suffix.Length == 0;

        if (etagSpan[position] != '-')
            return false;

        var suffixSegment = etagSpan[++position..];
        return suffixSegment.Equals(suffix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Generates an ETag based on the assembly timestamp, entity's last modified timestamp and an optional suffix.
    /// </summary>
    /// <param name="lastTimestamp">The last modified timestamp of the entity.</param>
    /// <param name="suffix">Optional suffix to include in the ETag (e.g., content hash).</param>
    /// <returns>
    /// ETag as string in the format: {assemblyTimestamp}-{lastTimestamp}[-{suffix}]
    /// </returns>
    public string Generate(ulong lastTimestamp, string suffix)
    {
        var timestampDigitCount = lastTimestamp.CountDigits();
        var totalLength = CalculateEtagLength(timestampDigitCount, suffix.Length);

        return string.Create(totalLength, (_assemblyTimestamp, lastTimestamp, suffix), (chars, state) =>
        {
            var (assemblyTimestamp, timestamp, suffix) = state;

            var position = assemblyTimestamp.Length;
            assemblyTimestamp.AsSpan().CopyTo(chars);

            chars[position++] = '-';

            timestamp.TryFormat(chars[position..], out var digitsWritten);
            position += digitsWritten;

            if (suffix.Length > 0)
            {
                chars[position++] = '-';
                suffix.AsSpan().CopyTo(chars[position..]);
            }
        });
    }

    /// <summary>
    /// Calculates the total length of an ETag based on timestamp digit count and suffix length.
    /// </summary>
    /// <param name="timestampDigitCount">The number of digits in the timestamp.</param>
    /// <param name="suffixLength">The length of the suffix string.</param>
    /// <returns>
    /// The total character length of the ETag including separators.
    /// </returns>
    /// <remarks>
    /// Calculation formula: assemblyTimestamp.Length + timestampDigitCount + suffixLength + separatorCount
    /// where separatorCount is 2 if suffixLength > 0 (two hyphens), otherwise 1 (one hyphen).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int CalculateEtagLength(int timestampDigitCount, int suffixLength)
    {
        var separatorCount = suffixLength > 0 ? 2 : 1;
        return _assemblyTimestamp.Length + timestampDigitCount + suffixLength + separatorCount;
    }
}

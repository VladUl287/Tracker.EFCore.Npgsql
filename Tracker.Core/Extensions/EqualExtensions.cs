namespace Tracker.Core.Extensions;

internal static class EqualExtensions
{
    private const int ULongMaxLength = 20;

    internal static bool MatchesULong(this ReadOnlySpan<char> chars, ulong number)
    {
        if (chars is { Length: 0 or > ULongMaxLength })
            return false;

        ulong result = 0;
        foreach (var c in chars)
            result = result * 10 + (uint)(c - '0');

        return result == number;
    }
}

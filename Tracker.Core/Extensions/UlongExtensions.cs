namespace Tracker.Core.Extensions;

public static class UlongExtensions
{
    public static int CountDigits(this ulong n)
    {
        if (n == 0) return 1;
        return (int)Math.Floor(Math.Log10(n)) + 1;
    }
}

using System.Numerics;

namespace MemWatcher;

public static class Util
{
    public static ReadOnlySpan<T> SafeSlice<T>(ReadOnlySpan<T> span, uint from, uint size) => SafeSlice(span, (int)from, (int)size);
    public static ReadOnlySpan<T> SafeSlice<T>(ReadOnlySpan<T> span, int from, int size)
    {
        from = Math.Min(span.Length, from);
        size = Math.Min(span.Length - from, size);
        return span.Slice(from, size);
    }

    static readonly long MaxAgeTicks = TimeSpan.FromSeconds(3).Ticks;
    public static Vector4 ColorForAge(long ageInTicks)
    {
        if (ageInTicks >= MaxAgeTicks)
            return Vector4.One;

        var t = (float)ageInTicks / MaxAgeTicks;
        return new Vector4(1.0f, t, t, 1.0f);
    }
}
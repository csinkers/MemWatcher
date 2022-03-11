﻿namespace MemWatcher;

public static class Util
{
    public static ReadOnlySpan<T> SafeSlice<T>(ReadOnlySpan<T> span, uint from, uint size) => SafeSlice(span, (int)from, (int)size);
    public static ReadOnlySpan<T> SafeSlice<T>(ReadOnlySpan<T> span, int from, int size)
    {
        from = Math.Min(span.Length, from);
        size = Math.Min(span.Length - from, size);
        return span.Slice(from, size);
    }
}
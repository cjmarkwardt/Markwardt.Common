namespace Markwardt;

public static class MemoryExtensions
{
    public static ReadOnlyMemory<T> AsReadOnly<T>(this Memory<T> memory)
        => memory;

    public static ReadOnlySpan<T> AsReadOnly<T>(this Span<T> span)
        => span;

    public static IEnumerable<ReadOnlyMemory<T>> Subdivide<T>(this ReadOnlyMemory<T> data, int size)
    {
        for (int i = 0; i < data.Length; i += size)
        {
            yield return data.Slice(i, Math.Min(size, data.Length - i));
        }
    }
}
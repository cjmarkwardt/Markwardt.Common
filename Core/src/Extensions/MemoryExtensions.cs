namespace Markwardt;

public static class MemoryExtensions
{
    public static ReadOnlyMemory<T> AsReadOnly<T>(this Memory<T> memory)
        => memory;

    public static ReadOnlySpan<T> AsReadOnly<T>(this Span<T> span)
        => span;
}
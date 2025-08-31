namespace Markwardt;

public interface IBufferPointer<T> : IMemoryWriter<T>, IMemoryReader<T>
{
    IBuffer<T> Buffer { get; }

    int Position { get; set; }
}

public static class BufferPointerExtensions
{
    public static IBufferPointer<T> CreatePointer<T>(this IBuffer<T> buffer, int position = 0)
        => new BufferPointer<T>(buffer) { Position = 0 };

    public static int GetRemaining<T>(this IBufferPointer<T> pointer)
        => pointer.Buffer.Length - pointer.Position;

    public static void Reset<T>(this IBufferPointer<T> pointer)
        => pointer.Position = 0;
}

public class BufferPointer<T>(IBuffer<T> buffer) : IBufferPointer<T>
{
    public IBuffer<T> Buffer => buffer;

    public int Position { get; set; }

    public void Read(int length, MemoryConsumer<T> consumer)
    {
        consumer(buffer.Span.Slice(Position, length));
        Position += length;
    }

    public void Read(Span<T> destination)
    {
        buffer.Span.Slice(Position, destination.Length).CopyTo(destination);
        Position += destination.Length;
    }

    public void Write(int length, MemoryEditor<T> editor)
    {
        int overhead = length - this.GetRemaining();
        if (overhead > 0)
        {
            buffer.Extend(overhead);
        }

        editor(buffer.Span.Slice(Position, length));
        Position += length;
    }

    public void Write(ReadOnlySpan<T> source)
    {
        int overhead = source.Length - this.GetRemaining();
        if (overhead > 0)
        {
            buffer.Extend(overhead);
        }

        source.CopyTo(buffer.Span.Slice(Position, source.Length));
        Position += source.Length;
    }
}
namespace Markwardt;

public interface IBuffer<T> : IMemoryWriteable<T>
{
    Memory<T> Memory { get; }
    Span<T> Span { get; }

    T this[int index] { get; set; }

    int Length { get; set; }
    int Capacity { get; set; }

    void Clear();
    void Resize(int length, bool keepData = false, bool exact = false, bool shrink = false);
}

public static class BufferExtensions
{
    public static MemoryWriteStream ToStream(this IBuffer<byte> buffer)
        => new(buffer);

    public static void Extend<T>(this IBuffer<T> buffer, int length, bool exact = false)
        => buffer.Resize(buffer.Length + length, true, exact);

    public static void ClampCapacity<T>(this IBuffer<T> buffer, int maxCapacity)
    {
        if (buffer.Capacity > maxCapacity)
        {
            buffer.Resize(maxCapacity, false, true, true);
        }
    }

    public static void Reset<T>(this IBuffer<T> buffer, bool resetCapacity = false)
    {
        buffer.Length = 0;

        if (resetCapacity)
        {
            buffer.Capacity = 0;
        }
    }

    public static void Fill<T>(this IBuffer<T> buffer, ReadOnlySpan<T> data, bool exact = false, bool shrink = false)
    {
        buffer.Resize(data.Length, false, exact, shrink);
        data.CopyTo(buffer.Span);
    }

    public static void Fill(this IBuffer<byte> buffer, Stream stream, bool exact = false, bool shrink = false)
    {
        buffer.Resize((int)stream.GetRemaining(), false, exact, shrink);
        stream.ReadExactly(buffer.Span);
    }

    public static async ValueTask FillAsync(this IBuffer<byte> buffer, Stream stream, bool exact = false, bool shrink = false)
    {
        buffer.Resize((int)stream.GetRemaining(), false, exact, shrink);
        await stream.ReadExactlyAsync(buffer.Memory);
    }

    public static void Append<T>(this IBuffer<T> buffer, T value, bool exact = false)
    {
        buffer.Resize(buffer.Length + 1, true, exact);
        buffer[^1] = value;
    }

    public static void Append<T>(this IBuffer<T> buffer, ReadOnlySpan<T> data, bool exact = false)
    {
        int oldLength = buffer.Length;
        buffer.Resize(buffer.Length + data.Length, true, exact);
        data.CopyTo(buffer.Span[oldLength..]);
    }

    public static void Append(this IBuffer<byte> buffer, Stream stream, bool exact = false)
    {
        int oldLength = buffer.Length;
        buffer.Resize(buffer.Length + (int)stream.GetRemaining(), true, exact);
        stream.ReadExactly(buffer.Span[oldLength..]);
    }

    public static async ValueTask AppendAsync(this IBuffer<byte> buffer, Stream stream, bool exact = false)
    {
        int oldLength = buffer.Length;
        buffer.Resize(buffer.Length + (int)stream.GetRemaining(), true, exact);
        await stream.ReadExactlyAsync(buffer.Memory[oldLength..]);
    }
}

public class Buffer<T>(int capacity) : IBuffer<T>
{
    public static T[] CreateArray(Action<IMemoryWriteable<T>> write)
    {
        Buffer<T> buffer = new();
        write(buffer);
        return buffer.ToArray();
    }

    public Buffer()
        : this(0) { }

    private T[] buffer = new T[capacity];

    public Memory<T> Memory => buffer.AsMemory()[..Length];
    public Span<T> Span => buffer.AsSpan()[..length];

    public BufferGrower? Grower { get; set; }

    public T this[int index]
    {
        get => Span[index];
        set => Span[index] = value;
    }

    private int length = 0;
    public int Length
    {
        get => length;
        set
        {
            if (value > Capacity)
            {
                throw new InvalidOperationException("Length cannot be greater than capacity");
            }
            else if (value < 0)
            {
                throw new InvalidOperationException("Length cannot be negative");
            }

            length = value;
        }
    }

    public int Capacity
    {
        get => buffer.Length;
        set
        {
            if (value < length)
            {
                throw new InvalidOperationException("Capacity cannot be less than length.");
            }

            if (buffer.Length != value)
            {
                buffer = new T[value];
            }
        }
    }

    public T[] ToArray()
    {
        T[] array = new T[length];
        Span.CopyTo(array);
        return array;
    }

    public void Clear()
        => buffer.AsSpan().Clear();

    public void Resize(int length, bool keepData = false, bool exact = false, bool shrink = false)
    {
        if (length > Length)
        {
            if (length > Capacity)
            {
                Grow(length, keepData, exact);
            }

            int oldLength = Length;
            Length = length;
            buffer.AsSpan()[oldLength..Length].Clear();
        }
        else if (length < Length)
        {
            Length = length;

            if (shrink)
            {
                Capacity = Length;
            }
        }
    }

    private void Grow(int minimum, bool keepData, bool exact)
    {
        if (Capacity == 0)
        {
            Capacity = minimum;
        }
        else
        {
            Memory<T> oldBuffer = buffer;
            if (exact)
            {
                Capacity = minimum;
            }
            else if (Grower is not null)
            {
                Capacity = Grower(minimum);
            }
            else
            {
                int newCapacity = Capacity;
                while (minimum > newCapacity)
                {
                    newCapacity *= 2;
                }

                Capacity = newCapacity;
            }

            if (keepData)
            {
                oldBuffer.CopyTo(buffer);
            }
        }
    }

    void IMemoryWriteable<T>.Write(ReadOnlySpan<T> source)
        => this.Append(source);

    void IMemoryWriteable<T>.Write(int length, MemoryEditor<T> editor)
    {
        this.Extend(length);
        editor(Span[^length..]);
    }
}
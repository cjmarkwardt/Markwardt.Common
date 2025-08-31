namespace Markwardt;

public interface IBuffer<T> : IMemoryWriter<T>
{
    Memory<T> Memory { get; }
    Span<T> Span { get; }

    T this[int index] { get; set; }

    int Length { get; set; }
    int Capacity { get; set; }

    void Clear();
    void Resize(int length, bool keepData = false, bool growExponential = true, bool shrink = false);
}

public static class BufferExtensions
{
    public static void Extend<T>(this IBuffer<T> buffer, int length, bool growExponential = true)
        => buffer.Resize(buffer.Length + length, true, growExponential);

    public static void ClampCapacity<T>(this IBuffer<T> buffer, int maxCapacity)
    {
        if (buffer.Capacity > maxCapacity)
        {
            buffer.Resize(maxCapacity, false, false, true);
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

    public static void Fill<T>(this IBuffer<T> buffer, ReadOnlySpan<T> data, bool growExponential = true, bool shrink = false)
    {
        buffer.Resize(data.Length, false, growExponential, shrink);
        data.CopyTo(buffer.Span);
    }

    public static void Fill(this IBuffer<byte> buffer, Stream stream, bool growExponential = true, bool shrink = false)
    {
        buffer.Resize((int)stream.GetRemaining(), false, growExponential, shrink);
        stream.ReadExactly(buffer.Span);
    }

    public static async ValueTask FillAsync(this IBuffer<byte> buffer, Stream stream, bool growExponential = true, bool shrink = false)
    {
        buffer.Resize((int)stream.GetRemaining(), false, growExponential, shrink);
        await stream.ReadExactlyAsync(buffer.Memory);
    }

    public static void Append<T>(this IBuffer<T> buffer, T value, bool growExponential = true)
    {
        buffer.Resize(buffer.Length + 1, true, growExponential);
        buffer[^1] = value;
    }

    public static void Append<T>(this IBuffer<T> buffer, ReadOnlySpan<T> data, bool growExponential = true)
    {
        int oldLength = buffer.Length;
        buffer.Resize(buffer.Length + data.Length, true, growExponential);
        data.CopyTo(buffer.Span[oldLength..]);
    }

    public static void Append(this IBuffer<byte> buffer, Stream stream, bool growExponential = true)
    {
        int oldLength = buffer.Length;
        buffer.Resize(buffer.Length + (int)stream.GetRemaining(), true, growExponential);
        stream.Write(buffer.Span[oldLength..]);
    }

    public static async ValueTask AppendAsync(this IBuffer<byte> buffer, Stream stream, bool growExponential = true)
    {
        int oldLength = buffer.Length;
        buffer.Resize(buffer.Length + (int)stream.GetRemaining(), true, growExponential);
        await stream.WriteAsync(buffer.Memory[oldLength..]);
    }
}

public class Buffer<T>(int capacity = 0) : IBuffer<T>
{
    private Memory<T> buffer = new T[capacity];

    public Memory<T> Memory => buffer[..Length];
    public Span<T> Span => buffer.Span[..length];

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
                throw new InvalidOperationException("Capacity cannot be less than length");
            }

            if (buffer.Length != value)
            {
                buffer = new T[value];
            }
        }
    }

    public void Clear()
        => buffer.Span.Clear();

    public void Resize(int length, bool keepData = false, bool growExponential = true, bool shrink = false)
    {
        if (length > Length)
        {
            if (length > Capacity)
            {
                GrowCapacity(length, keepData, growExponential);
            }

            int oldLength = Length;
            Length = length;
            buffer.Span[oldLength..Length].Clear();
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

    private void GrowCapacity(int minimumCapacity, bool keepData, bool exponential)
    {
        if (Capacity == 0)
        {
            Capacity = minimumCapacity;
        }
        else
        {
            Memory<T> oldBuffer = buffer;
            if (exponential)
            {
                int newCapacity = Capacity;
                while (minimumCapacity > newCapacity)
                {
                    newCapacity *= 2;
                }

                Capacity = newCapacity;
            }
            else
            {
                Capacity = minimumCapacity;
            }

            if (keepData)
            {
                oldBuffer.CopyTo(buffer);
            }
        }
    }

    void IMemoryWriter<T>.Write(ReadOnlySpan<T> source)
        => this.Append(source);

    void IMemoryWriter<T>.Write(int length, MemoryEditor<T> editor)
    {
        this.Extend(length);
        editor(Span[^length..]);
    }
}
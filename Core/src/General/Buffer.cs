namespace Markwardt;

public static class BufferExtensions
{
    public static Buffer<T> NewBuffer<T>(this MemoryPool<T>? pool, int? minCapacity = null)
        => Buffer<T>.New(pool, minCapacity);

    public static Buffer<T> ToBuffer<T>(this ReadOnlySpan<T> span, MemoryPool<T>? pool = null)
    {
        Buffer<T> buffer = Buffer<T>.New(pool, span.Length);
        buffer.Length = span.Length;
        span.CopyTo(buffer.Memory.Span);
        return buffer;
    }
}

public class Buffer<T> : IRecyclable
{
    private static readonly Pool<Buffer<T>> bufferPool = new(() => new Buffer<T>());

    public static CapacityGrower DefaultGrower { get; } = typeof(T) == typeof(byte) ? CreateGrower(256, 2) : CreateGrower(4, 2);

    public static CapacityGrower CreateGrower(int initialCapacity, float growth)
        => (oldCapacity, newLength) =>
        {
            int newCapacity = (int)(oldCapacity * growth);
            if (newCapacity == 0)
            {
                newCapacity = initialCapacity;
            }

            while (newCapacity < newLength)
            {
                newCapacity = (int)(newCapacity * growth);
            }

            return newCapacity;
        };

    public static Buffer<T> New(MemoryPool<T>? pool = null, int? minCapacity = null)
    {
        Buffer<T> buffer = bufferPool.Get();
        buffer.Pool = pool;
        
        if (minCapacity.HasValue)
        {
            buffer.ReplaceBuffer(minCapacity.Value, true);
        }

        return buffer;
    }

    private Buffer() { }

    private IMemoryOwner<T>? buffer;

    public Memory<T> Memory => buffer is null ? Memory<T>.Empty : buffer.Memory[..Length];
    public int Capacity => buffer?.Memory.Length ?? 0;

    public MemoryPool<T>? Pool { get; set; }
    public CapacityGrower? Grower { get; set; }

    private int length;
    public int Length
    {
        get => length;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            else if (value > Capacity)
            {
                ReplaceBuffer((Grower ?? DefaultGrower)(Capacity, value), true);
            }

            length = value;
        }
    }

    public void Cut(int maxCapacity)
    {
        if (Capacity > maxCapacity)
        {
            length = Math.Min(length, maxCapacity);

            ReplaceBuffer(maxCapacity, true);
        }
    }
    
    public void Reset(int? maxCapacity = null)
    {
        length = 0;

        if (maxCapacity.HasValue && Capacity > maxCapacity.Value)
        {
            ReplaceBuffer(maxCapacity.Value, false);
        }
    }

    public void Recycle()
    {
        buffer?.Dispose();
        buffer = null;
        length = 0;
        Pool = null;

        bufferPool.Recycle(this);
    }

    private void ReplaceBuffer(int minCapacity, bool copy)
    {
        IMemoryOwner<T> newBuffer = Pool?.Rent(minCapacity) ?? new MemoryOwner(minCapacity);
            
        if (buffer is not null)
        {
            if (copy)
            {
                Memory.CopyTo(newBuffer.Memory);
            }

            buffer.Dispose();
        }

        buffer = newBuffer;
    }

    private sealed class MemoryOwner(int capacity) : IMemoryOwner<T>
    {
        public Memory<T> Memory { get; } = new T[capacity];

        public void Dispose() { }
    }
}

/*public interface IBuffer<T>
{
    Memory<T> Memory { get; }
    int Capacity { get; }

    int Length { get; set; }

    void Reset(Memory<T> newBuffer);
}

public static class BufferExtensions
{
    public static void Reset<T>(this IBuffer<T> buffer, int capacity)
        => buffer.Reset(new T[capacity]);

    public static void Replace<T>(this IBuffer<T> buffer, Memory<T> memory)
    {
        if (buffer.Length > memory.Length)
        {
            throw new InvalidOperationException("Cannot replace with a smaller buffer.");
        }

        buffer.Memory.CopyTo(memory);
        buffer.Reset(memory);
    }

    public static void Replace<T>(this IBuffer<T> buffer, int capacity)
        => buffer.Replace(new T[capacity]);

    public static void Clamp<T>(this IBuffer<T> buffer, int maxCapacity)
    {
        if (buffer.Capacity > maxCapacity)
        {
            buffer.Reset(maxCapacity);
        }
        else
        {
            buffer.Length = 0;
        }
    }
}

public class Buffer<T>(int capacity) : IBuffer<T>
{
    public Buffer()
        : this(0) { }

    private Memory<T> buffer = new T[capacity];

    public Memory<T> Memory => buffer[..Length];
    public int Capacity => buffer.Length;

    private int length;
    public int Length
    {
        get => length;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            else if (value > Capacity)
            {
                this.Replace(GetGrowCapacity(value));
            }

            length = value;
        }
    }

    public void Reset(Memory<T> newBuffer)
    {
        buffer = newBuffer;
        length = 0;
    }

    protected virtual int GetGrowCapacity(int newLength)
    {
        int newCapacity = Capacity * 2;
        if (newCapacity == 0)
        {
            newCapacity = typeof(T) == typeof(byte) ? 256 : 4;
        }

        while (newCapacity < newLength)
        {
            newCapacity *= 2;
        }

        return newCapacity;
    }
}*/
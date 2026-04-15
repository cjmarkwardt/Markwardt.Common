namespace Markwardt;

public class MemoryBufferStream<T> : IRecyclable
{
    private static readonly Pool<MemoryBufferStream<T>> pool = new(() => new());

    public static MemoryBufferStream<T> New(Buffer<T> buffer)
    {
        MemoryBufferStream<T> stream = pool.Get();
        stream.Buffer = buffer;
        return stream;
    }

    public static MemoryBufferStream<T> New(MemoryPool<T>? pool = null, int? minCapacity = null)
        => New(Buffer<T>.New(pool, minCapacity));

    public MemoryBufferStream(Buffer<T> buffer)
        => this.buffer = buffer;

    public MemoryBufferStream(MemoryPool<T>? pool = null, int? minCapacity = null)
        : this(Buffer<T>.New(pool, minCapacity)) { }

    private MemoryBufferStream()
        => buffer = default!;

    private Buffer<T> buffer;
    public Buffer<T> Buffer
    {
        get => buffer;
        set
        {
            buffer = value;
            position = 0;
        }
    }

    public CapacityGrower? CapacityGrower { get => Buffer.Grower; set => Buffer.Grower = value; }

    public Memory<T> Memory => Buffer.Memory;
    public int Capacity => Buffer.Capacity;
    public int Length => Buffer.Length;

    private int position;
    public int Position
    {
        get => position;
        set
        {
            if (value < 0 || value > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            position = value;
        }
    }

    public int Seek(int offset, SeekOrigin origin)
        => Position = offset + origin switch
        {
            SeekOrigin.Begin => 0,
            SeekOrigin.Current => Position,
            SeekOrigin.End => Length,
            _ => throw new ArgumentOutOfRangeException(nameof(origin)),
        };

    public void SetLength(int value)
    {
        Buffer.Length = value;
        Position = Math.Min(Position, value);
    }

    public int Read(T[] buffer, int offset, int count)
    {
        int read = Math.Min(count, Length - Position);
        if (read > 0)
        {
            GetBlock(count).CopyTo(buffer.AsMemory(offset, read));
            Position += read;
        }

        return read;
    }

    public int Read(Span<T> buffer)
    {
        int read = Math.Min(buffer.Length, Length - Position);
        if (read > 0)
        {
            GetBlock(read).Span.CopyTo(buffer);
            Position += read;
        }

        return read;
    }

    public void Write(ReadOnlySpan<T> buffer)
    {
        if (Buffer.Length < Position + buffer.Length)
        {   
            Buffer.Length = Position + buffer.Length;
        }

        buffer.CopyTo(GetBlock(buffer.Length).Span);
        Position += buffer.Length;
    }

    private Memory<T> GetBlock(int count)
        => Memory.Slice(Position, count);

    public void Recycle()
    {
        buffer.Recycle();
        buffer = default!;
        
        pool.Recycle(this);
    }
}

public class MemoryBufferStream : Stream, IRecyclable
{
    private static readonly Pool<MemoryBufferStream> pool = new(() => new());

    public static MemoryBufferStream New(Buffer<byte> buffer)
    {
        MemoryBufferStream stream = pool.Get();
        stream.stream = MemoryBufferStream<byte>.New(buffer);
        return stream;
    }

    public static MemoryBufferStream New(MemoryPool<byte>? pool = null, int? minCapacity = null)
        => New(Buffer<byte>.New(pool, minCapacity));

    public MemoryBufferStream(Buffer<byte> buffer)
        => stream = MemoryBufferStream<byte>.New(buffer);

    public MemoryBufferStream(MemoryPool<byte>? pool = null, int? minCapacity = null)
        : this(pool.NewBuffer(minCapacity)) { }

    private MemoryBufferStream()
        => stream = default!;

    private MemoryBufferStream<byte> stream;

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => true;
    public override long Length => stream.Length;

    public Memory<byte> Memory => stream.Memory;
    public int Capacity => stream.Capacity;

    public Buffer<byte> Buffer { get => stream.Buffer; set => stream.Buffer = value; }
    public CapacityGrower? CapacityGrower { get => stream.CapacityGrower; set => stream.CapacityGrower = value; }
    public override long Position { get => stream.Position; set => stream.Position = (int)value; }

    public override void Flush() { }

    public override long Seek(long offset, SeekOrigin origin)
        => stream.Seek((int)offset, origin);

    public override void SetLength(long value)
        => stream.SetLength((int)value);

    public override int Read(byte[] buffer, int offset, int count)
        => stream.Read(buffer, offset, count);

    public override void Write(byte[] buffer, int offset, int count)
        => stream.Write(buffer.AsSpan(offset, count));

    public void Recycle()
    {
        stream.Recycle();
        stream = default!;

        pool.Recycle(this);
    }
}
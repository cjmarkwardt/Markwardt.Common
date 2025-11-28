namespace Markwardt;

public class MemoryWriteStream(IBuffer<byte> buffer) : BaseMemoryReadStream
{
    public MemoryWriteStream(int capacity)
        : this(new Buffer<byte>(capacity)) { }

    public MemoryWriteStream()
        : this(0) { }

    private IBuffer<byte> buffer = buffer;
    public IBuffer<byte> Buffer
    {
        get => buffer;
        set
        {
            buffer = value;
            Position = 0;
        }
    }

    public override bool CanWrite => true;

    public override void SetLength(long value)
    {
        Buffer.Resize((int)value);
        Position = Math.Min(Position, value);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        Buffer.Append(buffer);
        Position += buffer.Length;
    }

    public override void Write(byte[] buffer, int offset, int count)
        => Write(buffer.AsSpan(offset, count));

    protected override ReadOnlyMemory<byte> GetMemory()
        => Buffer.Memory;
}
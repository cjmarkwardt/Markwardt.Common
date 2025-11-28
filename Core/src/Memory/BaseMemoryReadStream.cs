namespace Markwardt;

public abstract class BaseMemoryReadStream : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => GetMemory().Length;

    public override long Position { get; set; }

    public override void Flush() { }

    public override int Read(Span<byte> buffer)
    {
        int bytesToRead = (int)Math.Min(buffer.Length, this.GetRemaining());
        GetMemory().Span.Slice((int)Position, bytesToRead).CopyTo(buffer);
        Position += bytesToRead;
        return bytesToRead;
    }

    public override int Read(byte[] buffer, int offset, int count)
        => Read(buffer.AsSpan(offset, count));

    public override long Seek(long offset, SeekOrigin origin)
    {
        int newPosition = origin switch
        {
            SeekOrigin.Begin => (int)offset,
            SeekOrigin.Current => (int)(Position + offset),
            SeekOrigin.End => (int)(Length + offset),
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };

        if (newPosition < 0 || newPosition > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Attempted to seek outside the bounds of the stream");
        }

        Position = newPosition;
        return Position;
    }

    public override void SetLength(long value)
        => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();

    protected abstract ReadOnlyMemory<byte> GetMemory();
}
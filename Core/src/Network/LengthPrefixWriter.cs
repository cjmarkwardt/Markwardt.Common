namespace Markwardt;

public interface ILengthPrefixWriter
{
    Buffer<byte> WriteStream(MemoryPool<byte>? pool, Action<Stream> write, int lengthEstimate = 0, bool prefixLength = true);
    Buffer<byte> WriteMemory(MemoryPool<byte>? pool, int length, Action<Memory<byte>> write, bool prefixLength = true);
    (int Length, ReadOnlyMemory<byte> Data) Read(ReadOnlyMemory<byte> content, bool prefixLength = true);
}

public static class LengthPrefixWriterExtensions
{
    public static ReadOnlyMemory<byte> ReadData(this ILengthPrefixWriter writer, ReadOnlyMemory<byte> content, bool prefixLength = true)
        => writer.Read(content, prefixLength).Data;
}

public class LengthPrefixWriter : ILengthPrefixWriter
{
    public static LengthPrefixWriter Default { get; } = new();

    public (int Length, ReadOnlyMemory<byte> Data) Read(ReadOnlyMemory<byte> content, bool prefixLength = true)
    {
        if (prefixLength)
        {
            return (BitConverter.ToInt32(content.Span), content[4..]);
        }
        else
        {
            return (-1, content);
        }
    }

    public Buffer<byte> WriteStream(MemoryPool<byte>? pool, Action<Stream> write, int lengthEstimate = 0, bool prefixLength = true)
    {
        MemoryBufferStream stream = new(pool, prefixLength ? lengthEstimate + 4 : lengthEstimate);

        if (prefixLength)
        {
            stream.SetLength(4);
            stream.Position = 4;
        }

        write(stream);

        if (prefixLength)
        {
            BitConverter.TryWriteBytes(stream.Buffer.Memory.Span, (int)stream.Length - 4);
        }

        return stream.Buffer;
    }

    public Buffer<byte> WriteMemory(MemoryPool<byte>? pool, int length, Action<Memory<byte>> write, bool prefixLength = true)
    {
        int bufferLength = prefixLength ? length + 4 : length;
        Buffer<byte> buffer = pool.NewBuffer(bufferLength);
        buffer.Length = bufferLength;

        Memory<byte> data = buffer.Memory;
        if (prefixLength)
        {
            BitConverter.TryWriteBytes(data.Span, length);
            data = data[4..];
        }

        write(data);

        return buffer;
    }
}
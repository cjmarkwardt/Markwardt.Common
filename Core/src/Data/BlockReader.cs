namespace Markwardt;

public interface IBlockReader
{
    ValueTask<byte?> Peek(CancellationToken cancellation = default);
    ValueTask Read(Memory<byte> destination, CancellationToken cancellation = default);
}

public static class BlockReaderExtensions
{
    public static async ValueTask<T> Read<T>(this IBlockReader reader, int length, Func<ReadOnlyMemory<byte>, T> read, int? maximumBufferLength = null, CancellationToken cancellation = default)
        => await ArrayPool<byte>.Shared.UseBuffer(length, async buffer =>
        {
            await reader.Read(buffer, cancellation);
            return read(buffer);
        }, maximumBufferLength ?? 64);
}

public class BlockReader(Stream stream) : IBlockReader
{
    private readonly Memory<byte> peekBuffer = new byte[1];

    public async ValueTask<byte?> Peek(CancellationToken cancellation = default)
    {
        try
        {
            await stream.ReadExactlyAsync(peekBuffer, cancellation);
        }
        catch (EndOfStreamException)
        {
            return null;
        }
        
        stream.Position--;
        return peekBuffer.Span[0];
    }

    public async ValueTask Read(Memory<byte> destination, CancellationToken cancellation = default)
    {
        try
        {
            await stream.ReadExactlyAsync(destination, cancellation);
        }
        catch (EndOfStreamException)
        {
            throw new EndOfDataException();
        }
    }
}
namespace Markwardt;

public interface IBlockReader
{
    ValueTask<byte?> Peek();
    ValueTask Read(Memory<byte> destination);
}

public static class BlockReaderExtensions
{
    public static async ValueTask<T> Read<T>(this IBlockReader reader, int length, Func<ReadOnlyMemory<byte>, T> read, int? maximumBufferLength = null)
        => await ArrayPool<byte>.Shared.UseBuffer(length, async buffer =>
        {
            await reader.Read(buffer);
            return read(buffer);
        }, maximumBufferLength ?? 64);
}

public class BlockReader(Stream stream) : IBlockReader
{
    private readonly Memory<byte> peekBuffer = new byte[1];

    public async ValueTask<byte?> Peek()
    {
        try
        {
            await stream.ReadExactlyAsync(peekBuffer);
        }
        catch (EndOfStreamException)
        {
            return null;
        }
        
        stream.Position--;
        return peekBuffer.Span[0];
    }

    public async ValueTask Read(Memory<byte> destination)
    {
        try
        {
            await stream.ReadExactlyAsync(destination);
        }
        catch (EndOfStreamException)
        {
            throw new EndOfDataException();
        }
    }
}
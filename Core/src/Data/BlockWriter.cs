namespace Markwardt;

public interface IBlockWriter
{
    ValueTask Write(ReadOnlyMemory<byte> source);
}

public static class BlockWriterExtensions
{
    public static async ValueTask Write(this IBlockWriter writer, int length, Action<Memory<byte>> write, int? maximumBufferLength = null)
        => await ArrayPool<byte>.Shared.UseBuffer(length, async buffer =>
        {
            write(buffer);
            await writer.Write(buffer);
        }, maximumBufferLength ?? 64);
}

public class BlockWriter(Stream stream) : IBlockWriter
{
    private readonly Stream stream = stream;

    public async ValueTask Write(ReadOnlyMemory<byte> source)
    {
        if (source.IsEmpty)
        {
            return;
        }

        await stream.WriteAsync(source);
    }
}
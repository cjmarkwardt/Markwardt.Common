namespace Markwardt;

public static class StreamExtensions
{
    public static async ValueTask ReadAtLeastAsync(this Stream stream, Memory<byte> buffer, bool throwOnEndOfStream = true, CancellationToken cancellationToken = default)
        => await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream, cancellationToken);

    public static async ValueTask<byte[]> CopyToArray(this Stream stream)
    {
        using MemoryStream buffer = new();
        await stream.CopyToAsync(buffer);
        return buffer.ToArray();
    }

    public static long GetRemaining(this Stream stream)
        => stream.Length - stream.Position;
}
namespace Markwardt;

public static class StreamExtensions
{
    public static async ValueTask ReadAtLeastAsync(this Stream stream, Memory<byte> buffer, bool throwOnEndOfStream = true, CancellationToken cancellationToken = default)
        => await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream, cancellationToken);

    public static async ValueTask ReadExactlyAsync(this Stream stream, Stream output, long? length = null, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
    {
        buffer ??= new byte[8192];

        long remaining = length ?? output.GetRemaining();
        while (remaining > 0)
        {
            int readLength = (int)Math.Min(int.MaxValue, Math.Min(buffer.Value.Length, remaining));
            Memory<byte> data = buffer.Value[0..readLength];
            await stream.ReadExactlyAsync(data, cancellationToken);
            await output.WriteAsync(data, cancellationToken);
            remaining -= readLength;
        }
    }

    public static void WriteInteger(this Stream stream, int value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BitConverter.TryWriteBytes(buffer, value);
        stream.Write(buffer);
    }

    public static async ValueTask WriteIntegerAsync(this Stream stream, int value, Memory<byte>? buffer = null, CancellationToken cancellation = default)
    {
        buffer ??= new byte[4];
        BitConverter.TryWriteBytes(buffer.Value.Span, value);
        await stream.WriteAsync(buffer.Value, cancellation);
    }

    public static int ReadInteger(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BitConverter.ToInt32(buffer);
    }

    public static async ValueTask<int> ReadIntegerAsync(this Stream stream, Memory<byte>? buffer = null, CancellationToken cancellation = default)
    {
        buffer ??= new byte[4];
        await stream.ReadExactlyAsync(buffer.Value, cancellation);
        return BitConverter.ToInt32(buffer.Value.Span);
    }

    public static async ValueTask<byte[]> CopyToArray(this Stream stream)
    {
        using MemoryStream buffer = new();
        await stream.CopyToAsync(buffer);
        return buffer.ToArray();
    }

    public static long GetRemaining(this Stream stream)
        => stream.Length - stream.Position;
}
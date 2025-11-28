namespace Markwardt;

public interface IDataPartReader
{
    ValueTask<object?> Read(CancellationToken cancellation = default);
}

public static class DataPartReaderExtensions
{
    public static async ValueTask<T> Read<T>(this IDataPartReader reader, CancellationToken cancellation = default)
        => (T)(await reader.Read(cancellation))!;
}

public class DataPartReader(IBlockReader reader) : IDataPartReader
{
    public async ValueTask<object?> Read(CancellationToken cancellation = default)
    {
        byte value = await ReadByte(cancellation);
        if (!value.GetBit(7))
        {
            if (!value.GetBit(6))
            {
                return (BigInteger)value - 1;
            }
            else
            {
                int length = value.ClearBit(6);
                return await reader.Read(length, buffer => new BigInteger(buffer.Span), cancellation: cancellation);
            }
        }
        else
        {
            return value.ClearBit(7) switch
            {
                (byte)BasicDataCode.Null => null,
                (byte)BasicDataCode.Single => await reader.Read(4, buffer => BitConverter.ToSingle(buffer.Span), cancellation: cancellation),
                (byte)BasicDataCode.Double => await reader.Read(8, buffer => BitConverter.ToDouble(buffer.Span), cancellation: cancellation),
                (byte)BasicDataCode.String => await ReadString(cancellation),
                byte code => new DataCode(code - (int)BasicDataCode.Length)
            };
        }
    }

    private async ValueTask<byte> ReadByte(CancellationToken cancellation = default)
        => await reader.Read(1, buffer => buffer.Span[0], cancellation: cancellation);

    private async ValueTask<string> ReadString(CancellationToken cancellation = default)
    {
        int length = await ReadByte(cancellation);
        int textLength = (int)await reader.Read(length, buffer => new BigInteger(buffer.Span, true), cancellation: cancellation);
        return await reader.Read(textLength, buffer => Encoding.UTF8.GetString(buffer.Span), cancellation: cancellation);
    }
}
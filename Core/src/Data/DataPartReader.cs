namespace Markwardt;

public interface IDataPartReader
{
    ValueTask<object?> Read();
}

public static class DataPartReaderExtensions
{
    public static async ValueTask<T> Read<T>(this IDataPartReader reader)
        => (T)(await reader.Read())!;
}

public class DataPartReader(IBlockReader reader) : IDataPartReader
{
    public async ValueTask<object?> Read()
    {
        byte value = await ReadByte();
        if (!value.GetBit(7))
        {
            if (!value.GetBit(6))
            {
                return (BigInteger)value - 1;
            }
            else
            {
                int length = value.ClearBit(6);
                return await reader.Read(length, buffer => new BigInteger(buffer.Span));
            }
        }
        else
        {
            return value.ClearBit(7) switch
            {
                (byte)BasicDataCode.Null => null,
                (byte)BasicDataCode.Single => await reader.Read(4, buffer => BitConverter.ToSingle(buffer.Span)),
                (byte)BasicDataCode.Double => await reader.Read(8, buffer => BitConverter.ToDouble(buffer.Span)),
                (byte)BasicDataCode.String => await ReadString(),
                byte code => new DataCode(code - (int)BasicDataCode.Length)
            };
        }
    }

    private async ValueTask<byte> ReadByte()
        => await reader.Read(1, buffer => buffer.Span[0]);

    private async ValueTask<string> ReadString()
    {
        int length = await ReadByte();
        int textLength = (int)await reader.Read(length, buffer => new BigInteger(buffer.Span, true));
        return await reader.Read(textLength, buffer => Encoding.UTF8.GetString(buffer.Span));
    }
}
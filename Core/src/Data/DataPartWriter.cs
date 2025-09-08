namespace Markwardt;

public interface IDataPartWriter
{
    ValueTask WriteInteger(BigInteger value);
    ValueTask WriteNull();
    ValueTask WriteSingle(float value);
    ValueTask WriteDouble(double value);
    ValueTask WriteString(string value);
    ValueTask WriteCode(int value);
}

public class DataPartWriter(IBlockWriter writer) : IDataPartWriter
{
    public async ValueTask WriteInteger(BigInteger value)
    {
        if (value < -1 || value > 62)
        {
            int length = value.GetByteCount();
            await WriteByte((byte)length.SetBit(6));
            await writer.Write(length, buffer => value.TryWriteBytes(buffer.Span, out _).Require());
        }
        else
        {
            await WriteByte((byte)(value + 1));
        }
    }

    public async ValueTask WriteNull()
        => await WriteRawCode((int)BasicDataCode.Null);

    public async ValueTask WriteSingle(float value)
    {
        await WriteRawCode((int)BasicDataCode.Single);
        await writer.Write(4, buffer => BitConverter.TryWriteBytes(buffer.Span, value));
    }

    public async ValueTask WriteDouble(double value)
    {
        await WriteRawCode((int)BasicDataCode.Double);
        await writer.Write(8, buffer => BitConverter.TryWriteBytes(buffer.Span, value));
    }

    public async ValueTask WriteString(string value)
    {
        await WriteRawCode((int)BasicDataCode.String);
        BigInteger textLength = Encoding.UTF8.GetByteCount(value);
        int length = (byte)textLength.GetByteCount(true);
        await WriteByte((byte)length);
        await writer.Write(length, buffer => textLength.TryWriteBytes(buffer.Span, out _, true).Require());
        await writer.Write((int)textLength, buffer => Encoding.UTF8.TryGetBytes(value, buffer.Span, out _));
    }

    public async ValueTask WriteCode(int value)
    {
        int max = 127 - (int)BasicDataCode.Length;
        if (value < 0 || value > max)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Signal value must be between 0 and {max}.");
        }

        await WriteRawCode(value + (int)BasicDataCode.Length);
    }

    private async ValueTask WriteRawCode(int value)
    {
        if (value < 0 || value > 127)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Raw code value must be between 0 and 127.");
        }

        await WriteByte((byte)value.SetBit(7));
    }

    private async ValueTask WriteByte(byte value)
        => await writer.Write(1, buffer => buffer.Span[0] = value);
}
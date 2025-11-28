namespace Markwardt;

public interface IDataPartWriter
{
    ValueTask WriteInteger(BigInteger value, CancellationToken cancellation = default);
    ValueTask WriteNull(CancellationToken cancellation = default);
    ValueTask WriteSingle(float value, CancellationToken cancellation = default);
    ValueTask WriteDouble(double value, CancellationToken cancellation = default);
    ValueTask WriteString(string value, CancellationToken cancellation = default);
    ValueTask WriteCode(int value, CancellationToken cancellation = default);
}

public class DataPartWriter(IBlockWriter writer) : IDataPartWriter
{
    public async ValueTask WriteInteger(BigInteger value, CancellationToken cancellation = default)
    {
        if (value < -1 || value > 62)
        {
            int length = value.GetByteCount();
            await WriteByte((byte)length.SetBit(6), cancellation);
            await writer.Write(length, buffer => value.TryWriteBytes(buffer.Span, out _).Require(), cancellation: cancellation);
        }
        else
        {
            await WriteByte((byte)(value + 1), cancellation);
        }
    }

    public async ValueTask WriteNull(CancellationToken cancellation = default)
        => await WriteRawCode((int)BasicDataCode.Null, cancellation);

    public async ValueTask WriteSingle(float value, CancellationToken cancellation = default)
    {
        await WriteRawCode((int)BasicDataCode.Single, cancellation);
        await writer.Write(4, buffer => BitConverter.TryWriteBytes(buffer.Span, value), cancellation: cancellation);
    }

    public async ValueTask WriteDouble(double value, CancellationToken cancellation = default)
    {
        await WriteRawCode((int)BasicDataCode.Double, cancellation);
        await writer.Write(8, buffer => BitConverter.TryWriteBytes(buffer.Span, value), cancellation: cancellation);
    }

    public async ValueTask WriteString(string value, CancellationToken cancellation = default)
    {
        await WriteRawCode((int)BasicDataCode.String, cancellation);
        BigInteger textLength = Encoding.UTF8.GetByteCount(value);
        int length = (byte)textLength.GetByteCount(true);
        await WriteByte((byte)length, cancellation);
        await writer.Write(length, buffer => textLength.TryWriteBytes(buffer.Span, out _, true).Require(), cancellation: cancellation);
        await writer.Write((int)textLength, buffer => Encoding.UTF8.TryGetBytes(value, buffer.Span, out _), cancellation: cancellation);
    }

    public async ValueTask WriteCode(int value, CancellationToken cancellation = default)
    {
        int max = 127 - (int)BasicDataCode.Length;
        if (value < 0 || value > max)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Signal value must be between 0 and {max}.");
        }

        await WriteRawCode(value + (int)BasicDataCode.Length, cancellation);
    }

    private async ValueTask WriteRawCode(int value, CancellationToken cancellation = default)
    {
        if (value < 0 || value > 127)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Raw code value must be between 0 and 127.");
        }

        await WriteByte((byte)value.SetBit(7), cancellation);
    }

    private async ValueTask WriteByte(byte value, CancellationToken cancellation = default)
        => await writer.Write(1, buffer => buffer.Span[0] = value, cancellation: cancellation);
}
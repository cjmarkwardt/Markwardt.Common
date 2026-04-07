namespace Markwardt;

public static class VariableInteger
{
    public static int GetLength(BigInteger value)
        => value >= -64 && value <= 63 ? 1 : 1 + value.GetByteCount();

    public static int Write(Span<byte> span, BigInteger value)
    {
        int written;
        if (value >= -64 && value <= 63)
        {
            written = 1;
            span[0] = ((byte)((int)value + 64)).SetBit(7);
        }
        else
        {
            int valueLength = value.GetByteCount();
            written = 1 + valueLength;
            span[0] = (byte)valueLength;
            value.TryWriteBytes(span[1..], out _);
        }

        return written;
    }

    public static BigInteger Read(ReadOnlySpan<byte> span, out int length)
    {
        if (span.GetBit(7))
        {
            length = 1;
            return span[0].ClearBit(7) - 64;
        }
        else
        {
            int valueLength = span[0];
            length = 1 + valueLength;
            return new BigInteger(span.Slice(1, valueLength));
        }
    }
}
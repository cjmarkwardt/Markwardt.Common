namespace Markwardt;

public class MemoryWriter<T>
{
    public int Position { get; set; }

    public void Write(Span<T> span, T value)
        => span[Position++] = value;

    public void Write(Span<T> span, int length, MemoryEditor<T> editor)
    {
        editor(span.Slice(Position, length));
        Position += length;
    }
}

public static class MemoryWriterr
{
    public static void WriteBoolean(this MemoryWriter<byte> writer, Span<byte> span, bool value)
        => writer.Write(span, (byte)(value ? 1 : 0));

    public static void WriteSigned(this MemoryWriter<byte> writer, Span<byte> span, sbyte value)
        => writer.Write(span, (byte)value);

    public static void WriteShort(this MemoryWriter<byte> writer, Span<byte> span, short value)
        => writer.Write(span, 2, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteUnsignedShort(this MemoryWriter<byte> writer, Span<byte> span, ushort value)
        => writer.Write(span, 2, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteInteger(this MemoryWriter<byte> writer, Span<byte> span, int value)
        => writer.Write(span, 4, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteUnsignedInteger(this MemoryWriter<byte> writer, Span<byte> span, uint value)
        => writer.Write(span, 4, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteLong(this MemoryWriter<byte> writer, Span<byte> span, long value)
        => writer.Write(span, 8, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteUnsignedLong(this MemoryWriter<byte> writer, Span<byte> span, ulong value)
        => writer.Write(span, 8, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteFloat(this MemoryWriter<byte> writer, Span<byte> span, float value)
        => writer.Write(span, 4, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteDouble(this MemoryWriter<byte> writer, Span<byte> span, double value)
        => writer.Write(span, 8, data => BitConverter.TryWriteBytes(data, value));

    /*
        Short 1 (-2:13)
        Medium 2:16 ()

        XXX Options
        X IsDirect
        1111 IsExtended

    */

    public static int GetVariableIntegerLength(this MemoryWriter<byte> writer, BigInteger value)
    {
        if (value >= -2 && value <= 13)
        {
            return 1;
        }
        else
        {
            int length = value.GetByteCount();
            if (length < 16)
            {
                return 1 + length;
            }
            else if (length < 16 + 255)
            {
                return 2 + length;
            }
            else
            {
                throw new InvalidOperationException($"Length cannot be larger than {16 + 255}");
            }
        }
    }

    public static void WriteVariableInteger(this MemoryWriter<byte> writer, Span<byte> span, BigInteger value, int embeddedValue = 0)
    {
        if (embeddedValue < 0 || embeddedValue > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(embeddedValue), "Embedded value must be between 0 and 7");
        }

        if (value >= -2 && value <= 13)
        {
            writer.Write(span, (byte)((int)(value + 2) | (embeddedValue << 5)));
        }
        else
        {
            int length = value.GetByteCount();
            int prefix = 0b10000 | (embeddedValue << 5);
            int? extendedLength = null;
            if (length < 16)
            {
                prefix |= length - 1;
            }
            else if (length < 16 + 255)
            {
                prefix |= 0b1111;
                extendedLength = length - 16;
            }
            else
            {
                throw new InvalidOperationException($"Length cannot be larger than {16 + 255}");
            }

            writer.Write(span, (byte)prefix);

            if (extendedLength is not null)
            {
                writer.Write(span, (byte)extendedLength.Value);
            }

            writer.Write(span, length, data => value.TryWriteBytes(data, out _));
        }
    }

    public static int GetBlockLength(this MemoryWriter<byte> writer, ReadOnlySpan<byte> data)
        => writer.GetVariableIntegerLength(data.Length) + data.Length;

    public static void WriteBlock(this MemoryWriter<byte> writer, Span<byte> span, ReadOnlyMemory<byte> data, int embeddedValue = 0)
    {
        writer.WriteVariableInteger(span, data.Length, embeddedValue);
        writer.Write(span, data.Length, span => data.Span.CopyTo(span));
    }

    public static int GetStringLength(this MemoryWriter<byte> writer, string value, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        int byteCount = encoding.GetByteCount(value);
        return writer.GetVariableIntegerLength(byteCount) + byteCount;
    }

    public static void WriteString(this MemoryWriter<byte> writer, Span<byte> span, string value, int embeddedValue = 0, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        int byteCount = encoding.GetByteCount(value);
        writer.WriteVariableInteger(span, byteCount, embeddedValue);
        writer.Write(span, byteCount, data => encoding.TryGetBytes(value, data, out _));
    }
}
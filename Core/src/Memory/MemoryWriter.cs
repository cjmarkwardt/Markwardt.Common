namespace Markwardt;

public interface IMemoryWriter<T>
{
    void Write(int length, MemoryEditor<T> editor);
    void Write(ReadOnlySpan<T> source);
}

public enum VariableIntegerOption
{
    Zero = 0,
    One = 0b1,
    Two = 0b10,
    Three = 0b11
}

public static class MemoryWriter
{
    public static void Write<T>(this Span<T> span, int index, out int newIndex, T value)
    {
        newIndex = index + 1;
        span[index] = value;
    }

    public static void Write<T>(this Span<T> span, int index, out int newIndex, int length, MemoryEditor<T> editor)
    {
        newIndex = index + length;
        editor(span.Slice(index, length));
    }

    public static void WriteBoolean(this Span<byte> span, int index, out int newIndex, bool value)
        => span.Write(index, out newIndex, (byte)(value ? 1 : 0));

    public static void WriteSigned(this Span<byte> span, int index, out int newIndex, sbyte value)
        => span.Write(index, out newIndex, (byte)value);

    public static void WriteShort(this Span<byte> span, int index, out int newIndex, short value)
        => span.Write(index, out newIndex, 2, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteUnsignedShort(this Span<byte> span, int index, out int newIndex, ushort value)
        => span.Write(index, out newIndex, 2, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteInteger(this Span<byte> span, int index, out int newIndex, int value)
        => span.Write(index, out newIndex, 4, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteUnsignedInteger(this Span<byte> span, int index, out int newIndex, uint value)
        => span.Write(index, out newIndex, 4, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteLong(this Span<byte> span, int index, out int newIndex, long value)
        => span.Write(index, out newIndex, 8, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteUnsignedLong(this Span<byte> span, int index, out int newIndex, ulong value)
        => span.Write(index, out newIndex, 8, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteFloat(this Span<byte> span, int index, out int newIndex, float value)
        => span.Write(index, out newIndex, 4, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteDouble(this Span<byte> span, int index, out int newIndex, double value)
        => span.Write(index, out newIndex, 8, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteVariableInteger(this Span<byte> span, int index, out int newIndex, BigInteger value, VariableIntegerOption option = VariableIntegerOption.Zero, bool isUnsigned = false)
    {
        newIndex = index;

        if (value >= 0 && value <= 31)
        {
            span.Write(newIndex, out newIndex, (byte)((int)value | ((int)option << 6)));
        }
        else
        {
            int length = value.GetByteCount();
            int prefix = 0b100000 | ((int)option << 6);
            if (length >= 31)
            {
                if (length > 286)
                {
                    throw new InvalidOperationException("Length cannot be larger than 286");
                }

                prefix |= 0b11111;
                span.Write(newIndex, out newIndex, (byte)(length - 31));
            }

            span.Write(newIndex, out newIndex, (byte)prefix);
            span.Write(newIndex, out newIndex, length, data => value.TryWriteBytes(data, out _, isUnsigned));
        }
    }

    public static void WriteString(this Span<byte> span, int index, out int newIndex, string value, VariableIntegerOption option = VariableIntegerOption.Zero, Encoding? encoding = null)
    {
        newIndex = index;

        span.WriteVariableInteger(newIndex, out newIndex, value.Length, option, true);
        encoding ??= Encoding.UTF8;
        span.Write(newIndex, out newIndex, encoding.GetByteCount(value), data => encoding.TryGetBytes(value, data, out _));
    }
}

public static class MemoryWriterExtensions
{
    public static void Write<T>(this IMemoryWriter<T> writer, T value)
        => writer.Write(1, data => data[0] = value);

    public static void WriteBoolean(this IMemoryWriter<byte> writer, bool value)
        => writer.Write((byte)(value ? 1 : 0));

    public static void WriteSigned(this IMemoryWriter<byte> writer, sbyte value)
        => writer.Write((byte)value);

    public static void WriteShort(this IMemoryWriter<byte> writer, short value)
        => writer.Write(2, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteUnsignedShort(this IMemoryWriter<byte> writer, ushort value)
        => writer.Write(2, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteInteger(this IMemoryWriter<byte> writer, int value)
        => writer.Write(4, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteUnsignedInteger(this IMemoryWriter<byte> writer, uint value)
        => writer.Write(4, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteLong(this IMemoryWriter<byte> writer, long value)
        => writer.Write(8, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteUnsignedLong(this IMemoryWriter<byte> writer, ulong value)
        => writer.Write(8, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteFloat(this IMemoryWriter<byte> writer, float value)
        => writer.Write(4, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteDouble(this IMemoryWriter<byte> writer, double value)
        => writer.Write(8, data => BitConverter.TryWriteBytes(data, value));

    public static void WriteVariableInteger(this IMemoryWriter<byte> writer, BigInteger value, VariableIntegerOption option = VariableIntegerOption.Zero, bool isUnsigned = false)
    {
        if (value >= 0 && value <= 31)
        {
            writer.Write((byte)((int)value | ((int)option << 6)));
        }
        else
        {
            int length = value.GetByteCount();
            int prefix = 0b100000 | ((int)option << 6);
            if (length >= 31)
            {
                if (length > 286)
                {
                    throw new InvalidOperationException("Length cannot be larger than 286");
                }

                prefix |= 0b11111;
                writer.Write((byte)(length - 31));
            }

            writer.Write((byte)prefix);
            writer.Write(length, data => value.TryWriteBytes(data, out _, isUnsigned));
        }
    }

    public static void WriteString(this IMemoryWriter<byte> writer, string value, VariableIntegerOption option = VariableIntegerOption.Zero, Encoding? encoding = null)
    {
        writer.WriteVariableInteger(value.Length, option, true);
        encoding ??= Encoding.UTF8;
        writer.Write(encoding.GetByteCount(value), data => encoding.TryGetBytes(value, data, out _));
    }
}
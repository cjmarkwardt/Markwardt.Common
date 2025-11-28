namespace Markwardt;

public interface IMemoryWriteable<T>
{
    void Write(int length, MemoryEditor<T> editor);
    void Write(ReadOnlySpan<T> source);
}

public static class MemoryWriteableExtensions
{
    private static readonly MemoryWriter<byte> binaryWriter = new();

    private static MemoryWriter<byte> GetBinaryWriter()
    {
        binaryWriter.Position = 0;
        return binaryWriter;
    }

    public static void Write<T>(this IMemoryWriteable<T> writeable, T value)
        => writeable.Write(1, data => data[0] = value);

    public static void WriteBoolean(this IMemoryWriteable<byte> writeable, bool value)
        => writeable.Write(1, data => GetBinaryWriter().WriteBoolean(data, value));

    public static void WriteSigned(this IMemoryWriteable<byte> writeable, sbyte value)
        => writeable.Write(1, data => GetBinaryWriter().WriteSigned(data, value));

    public static void WriteShort(this IMemoryWriteable<byte> writeable, short value)
        => writeable.Write(2, data => GetBinaryWriter().WriteShort(data, value));

    public static void WriteUnsignedShort(this IMemoryWriteable<byte> writeable, ushort value)
        => writeable.Write(2, data => GetBinaryWriter().WriteUnsignedShort(data, value));

    public static void WriteInteger(this IMemoryWriteable<byte> writeable, int value)
        => writeable.Write(4, data => GetBinaryWriter().WriteInteger(data, value));

    public static void WriteUnsignedInteger(this IMemoryWriteable<byte> writeable, uint value)
        => writeable.Write(4, data => GetBinaryWriter().WriteUnsignedInteger(data, value));

    public static void WriteLong(this IMemoryWriteable<byte> writeable, long value)
        => writeable.Write(8, data => GetBinaryWriter().WriteLong(data, value));

    public static void WriteUnsignedLong(this IMemoryWriteable<byte> writeable, ulong value)
        => writeable.Write(8, data => GetBinaryWriter().WriteUnsignedLong(data, value));

    public static void WriteFloat(this IMemoryWriteable<byte> writeable, float value)
        => writeable.Write(4, data => GetBinaryWriter().WriteFloat(data, value));

    public static void WriteDouble(this IMemoryWriteable<byte> writeable, double value)
        => writeable.Write(8, data => GetBinaryWriter().WriteDouble(data, value));

    public static void WriteVariableInteger(this IMemoryWriteable<byte> writeable, BigInteger value, int embeddedValue = 0)
        => writeable.Write(binaryWriter.GetVariableIntegerLength(value), data => GetBinaryWriter().WriteVariableInteger(data, value, embeddedValue));

    public static void WriteBlock(this IMemoryWriteable<byte> writeable, ReadOnlyMemory<byte> data, int embeddedValue = 0)
        => writeable.Write(binaryWriter.GetBlockLength(data.Span), span => GetBinaryWriter().WriteBlock(span, data, embeddedValue));

    public static void WriteString(this IMemoryWriteable<byte> writeable, string value, int embeddedValue = 0, Encoding? encoding = null)
        => writeable.Write(binaryWriter.GetStringLength(value, encoding), data => GetBinaryWriter().WriteString(data, value, embeddedValue, encoding));
}
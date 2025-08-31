namespace Markwardt;

public interface IMemoryReader<T>
{
    void Read(int length, MemoryConsumer<T> consumer);
    void Read(Span<T> destination);
}

public static class MemoryReader
{
    public static T Read<T>(this ReadOnlySpan<T> span, int index, out int newIndex)
    {
        newIndex = index + 1;
        return span[index];
    }

    public static TResult Read<T, TResult>(this ReadOnlySpan<T> span, int index, out int newIndex, int length, MemoryConverter<T, TResult> converter)
    {
        newIndex = index + length;
        return converter(span.Slice(index, length));
    }

    public static TResult Read<T, TResult>(this ReadOnlySpan<T> span, int index, out int newIndex, MemoryConverter<T, TResult> converter)
        => span.Read(index, out newIndex, span.Length, converter);

    public static bool ReadBoolean(this ReadOnlySpan<byte> span, int index, out int newIndex)
        => span.Read(index, out newIndex) == 1;

    public static sbyte ReadSigned(this ReadOnlySpan<byte> span, int index, out int newIndex)
        => (sbyte)span.Read(index, out newIndex);

    public static short ReadShort(this ReadOnlySpan<byte> span, int index, out int newIndex)
        => span.Read(index, out newIndex, 2, BitConverter.ToInt16);

    public static ushort ReadUnsignedShort(this ReadOnlySpan<byte> span, int index, out int newIndex)
        => span.Read(index, out newIndex, 2, BitConverter.ToUInt16);

    public static int ReadInteger(this ReadOnlySpan<byte> span, int index, out int newIndex)
        => span.Read(index, out newIndex, 4, BitConverter.ToInt32);

    public static uint ReadUnsignedInteger(this ReadOnlySpan<byte> span, int index, out int newIndex)
        => span.Read(index, out newIndex, 4, BitConverter.ToUInt32);

    public static long ReadLong(this ReadOnlySpan<byte> span, int index, out int newIndex)
        => span.Read(index, out newIndex, 8, BitConverter.ToInt64);

    public static ulong ReadUnsignedLong(this ReadOnlySpan<byte> span, int index, out int newIndex)
        => span.Read(index, out newIndex, 8, BitConverter.ToUInt64);

    public static float ReadFloat(this ReadOnlySpan<byte> span, int index, out int newIndex)
        => span.Read(index, out newIndex, 4, BitConverter.ToSingle);

    public static double ReadDouble(this ReadOnlySpan<byte> span, int index, out int newIndex)
        => span.Read(index, out newIndex, 8, BitConverter.ToDouble);

    public static BigInteger ReadVariableInteger(this ReadOnlySpan<byte> span, int index, out int newIndex, out VariableIntegerOption option, bool isUnsigned = false)
    {
        newIndex = index;

        int prefix = span.Read(index, out newIndex);
        option = (VariableIntegerOption)(prefix >> 6);
        int value = prefix & 0b00011111;

        if (!prefix.GetBit(5))
        {
            return value;
        }
        else
        {
            if (value == 31)
            {
                value += span.Read(newIndex, out newIndex);
            }

            return span.Read(newIndex, out newIndex, value, data => new BigInteger(data, isUnsigned));
        }
    }

    public static string ReadString(this ReadOnlySpan<byte> span, int index, out int newIndex, out VariableIntegerOption option, Encoding? encoding = null)
    {
        newIndex = index;

        BigInteger length = span.ReadVariableInteger(newIndex, out newIndex, out option, true);
        encoding ??= Encoding.UTF8;
        return span.Read(newIndex, out newIndex, (int)length, data => encoding.GetString(data));
    }
}

public static class MemoryReaderExtensions
{
    public static T Read<T>(this IMemoryReader<T> reader)
        where T : unmanaged
    {
        Span<T> span = stackalloc T[1];
        reader.Read(span);
        return span[0];
    }

    public static TResult Read<T, TResult>(this IMemoryReader<T> reader, int length, MemoryConverter<T, TResult> converter)
        => reader.Read<T, TResult>(length, data => MemoryReader.Read(data, 0, out _, converter));

    public static bool ReadBoolean(this IMemoryReader<byte> reader)
        => reader.Read() == 1;

    public static sbyte ReadSigned(this IMemoryReader<byte> reader)
        => (sbyte)reader.Read();

    public static short ReadShort(this IMemoryReader<byte> reader)
        => reader.Read(2, BitConverter.ToInt16);

    public static ushort ReadUnsignedShort(this IMemoryReader<byte> reader)
        => reader.Read(2, BitConverter.ToUInt16);

    public static int ReadInteger(this IMemoryReader<byte> reader)
        => reader.Read(4, BitConverter.ToInt32);

    public static uint ReadUnsignedInteger(this IMemoryReader<byte> reader)
        => reader.Read(4, BitConverter.ToUInt32);

    public static long ReadLong(this IMemoryReader<byte> reader)
        => reader.Read(8, BitConverter.ToInt64);

    public static ulong ReadUnsignedLong(this IMemoryReader<byte> reader)
        => reader.Read(8, BitConverter.ToUInt64);

    public static float ReadFloat(this IMemoryReader<byte> reader)
        => reader.Read(4, BitConverter.ToSingle);

    public static double ReadDouble(this IMemoryReader<byte> reader)
        => reader.Read(8, BitConverter.ToDouble);

    public static BigInteger ReadVariableInteger(this IMemoryReader<byte> reader, out VariableIntegerOption option, bool isUnsigned = false)
    {
        int prefix = reader.Read();
        option = (VariableIntegerOption)(prefix >> 6);
        int value = prefix & 0b00011111;

        if (!prefix.GetBit(5))
        {
            return value;
        }
        else
        {
            if (value == 31)
            {
                value += reader.Read();
            }

            return reader.Read<byte, BigInteger>(value, data => new BigInteger(data, isUnsigned));
        }
    }

    public static string ReadString(this IMemoryReader<byte> reader, out VariableIntegerOption option, Encoding? encoding = null)
    {
        BigInteger length = reader.ReadVariableInteger(out option, true);
        encoding ??= Encoding.UTF8;
        return reader.Read<byte, string>((int)length, data => encoding.GetString(data));
    }
}
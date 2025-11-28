namespace Markwardt;

public class MemoryReader<T>
{
    public int Position { get; set; }

    public T Read(ReadOnlySpan<T> span)
        => span[Position++];

    public TResult Read<TResult>(ReadOnlySpan<T> span, int length, MemoryConverter<T, TResult> converter)
    {
        TResult result = converter(span.Slice(Position, length));
        Position += length;
        return result;
    }
}

public static class MemoryReaderExtensions
{
    public static ReadOnlySpan<T> ReadRemaining<T>(this MemoryReader<T> reader, ReadOnlySpan<T> span)
    {
        ReadOnlySpan<T> result = span[reader.Position..];
        reader.Position += result.Length;
        return result;
    }

    public static ReadOnlyMemory<T> ReadRemaining<T>(this MemoryReader<T> reader, ReadOnlyMemory<T> memory)
    {
        ReadOnlyMemory<T> result = memory[reader.Position..];
        reader.Position += result.Length;
        return result;
    }

    public static bool ReadBoolean(this MemoryReader<byte> reader, ReadOnlySpan<byte> span)
        => reader.Read(span) == 1;

    public static sbyte ReadSigned(this MemoryReader<byte> reader, ReadOnlySpan<byte> span)
        => (sbyte)reader.Read(span);

    public static short ReadShort(this MemoryReader<byte> reader, ReadOnlySpan<byte> span)
        => reader.Read(span, 2, BitConverter.ToInt16);

    public static ushort ReadUnsignedShort(this MemoryReader<byte> reader, ReadOnlySpan<byte> span)
        => reader.Read(span, 2, BitConverter.ToUInt16);

    public static int ReadInteger(this MemoryReader<byte> reader, ReadOnlySpan<byte> span)
        => reader.Read(span, 4, BitConverter.ToInt32);

    public static uint ReadUnsignedInteger(this MemoryReader<byte> reader, ReadOnlySpan<byte> span)
        => reader.Read(span, 4, BitConverter.ToUInt32);

    public static long ReadLong(this MemoryReader<byte> reader, ReadOnlySpan<byte> span)
        => reader.Read(span, 8, BitConverter.ToInt64);

    public static ulong ReadUnsignedLong(this MemoryReader<byte> reader, ReadOnlySpan<byte> span)
        => reader.Read(span, 8, BitConverter.ToUInt64);

    public static float ReadFloat(this MemoryReader<byte> reader, ReadOnlySpan<byte> span)
        => reader.Read(span, 4, BitConverter.ToSingle);

    public static double ReadDouble(this MemoryReader<byte> reader, ReadOnlySpan<byte> span)
        => reader.Read(span, 8, BitConverter.ToDouble);

    public static BigInteger ReadVariableInteger(this MemoryReader<byte> reader, ReadOnlySpan<byte> span, out int embeddedValue)
    {
        int prefix = reader.Read(span);
        embeddedValue = prefix >> 5;
        int value = prefix & 0b00001111;

        if (!prefix.GetBit(4))
        {
            return value - 2;
        }
        else
        {
            value++;

            if (value == 16)
            {
                value += reader.Read(span);
            }

            return reader.Read(span, value, data => new BigInteger(data));
        }
    }

    public static BigInteger ReadVariableInteger(this MemoryReader<byte> reader, ReadOnlySpan<byte> span)
        => reader.ReadVariableInteger(span, out _);

    public static byte[] ReadBlock(this MemoryReader<byte> reader, ReadOnlySpan<byte> span, out int embeddedValue)
    {
        int length = (int)reader.ReadVariableInteger(span, out embeddedValue);
        byte[] block = new byte[length];
        span.Slice(reader.Position, length).CopyTo(block);
        reader.Position += length;
        return block;
    }

    public static byte[] ReadBlock(this MemoryReader<byte> reader, ReadOnlySpan<byte> span)
        => reader.ReadBlock(span, out _);

    public static ReadOnlyMemory<byte> ReadDirectBlock(this MemoryReader<byte> reader, ReadOnlyMemory<byte> memory, out int embeddedValue)
    {
        int length = (int)reader.ReadVariableInteger(memory.Span, out embeddedValue);
        ReadOnlyMemory<byte> result = memory.Slice(reader.Position, length);
        reader.Position += length;
        return result;
    }

    public static ReadOnlyMemory<byte> ReadDirectBlock(this MemoryReader<byte> reader, ReadOnlyMemory<byte> memory)
        => reader.ReadDirectBlock(memory, out _);

    public static string ReadString(this MemoryReader<byte> reader, ReadOnlySpan<byte> span, out int embeddedValue, Encoding? encoding = null)
        => reader.Read(span, (int)reader.ReadVariableInteger(span, out embeddedValue), data => (encoding ?? Encoding.UTF8).GetString(data));

    public static string ReadString(this MemoryReader<byte> reader, ReadOnlySpan<byte> span, Encoding? encoding = null)
        => reader.ReadString(span, out _, encoding);
}

/*public static class MemoryReaderrr
{
    public static T Read<T>(this ReadOnlySpan<T> span, out int newIndex)
    {
        newIndex = 1;
        return span[0];
    }

    public static TResult Read<T, TResult>(this ReadOnlySpan<T> span, out int newIndex, int length, MemoryConverter<T, TResult> converter)
    {
        newIndex = length;
        return converter(span[..length]);
    }

    public static TResult Read<T, TResult>(this ReadOnlySpan<T> span, out int newIndex, MemoryConverter<T, TResult> converter)
        => span.Read(out newIndex, span.Length, converter);

    public static bool ReadBoolean(this ReadOnlySpan<byte> span, out int newIndex)
        => span.Read(out newIndex) == 1;

    public static sbyte ReadSigned(this ReadOnlySpan<byte> span, out int newIndex)
        => (sbyte)span.Read(out newIndex);

    public static short ReadShort(this ReadOnlySpan<byte> span, out int newIndex)
        => span.Read(out newIndex, 2, BitConverter.ToInt16);

    public static ushort ReadUnsignedShort(this ReadOnlySpan<byte> span, out int newIndex)
        => span.Read(out newIndex, 2, BitConverter.ToUInt16);

    public static int ReadInteger(this ReadOnlySpan<byte> span, out int newIndex)
        => span.Read(out newIndex, 4, BitConverter.ToInt32);

    public static uint ReadUnsignedInteger(this ReadOnlySpan<byte> span, out int newIndex)
        => span.Read(out newIndex, 4, BitConverter.ToUInt32);

    public static long ReadLong(this ReadOnlySpan<byte> span, out int newIndex)
        => span.Read(out newIndex, 8, BitConverter.ToInt64);

    public static ulong ReadUnsignedLong(this ReadOnlySpan<byte> span, out int newIndex)
        => span.Read(out newIndex, 8, BitConverter.ToUInt64);

    public static float ReadFloat(this ReadOnlySpan<byte> span, out int newIndex)
        => span.Read(out newIndex, 4, BitConverter.ToSingle);

    public static double ReadDouble(this ReadOnlySpan<byte> span, out int newIndex)
        => span.Read(out newIndex, 8, BitConverter.ToDouble);

    public static BigInteger ReadVariableInteger(this ReadOnlySpan<byte> span, out int newIndex, out int embeddedValue)
    {
        newIndex = 0;

        int prefix = span[newIndex..].Read(out newIndex);
        embeddedValue = prefix >> 5;
        int value = prefix & 0b00001111;

        if (!prefix.GetBit(4))
        {
            return value - 2;
        }
        else
        {
            value++;

            if (value == 16)
            {
                value += span[newIndex..].Read(out newIndex);
            }

            return span[newIndex..].Read(out newIndex, value, data => new BigInteger(data));
        }
    }

    public static byte[] ReadBlock(this ReadOnlySpan<byte> span, out int newIndex, out int embeddedValue)
    {
        newIndex = 0;

        int length = (int)span[newIndex..].ReadVariableInteger(out newIndex, out embeddedValue);
        byte[] block = new byte[length];
        span[newIndex..].CopyTo(block);
        newIndex += length;
        return block;
    }

    public static string ReadString(this ReadOnlySpan<byte> span, out int newIndex, out int embeddedValue, Encoding? encoding = null)
    {
        newIndex = 0;

        BigInteger length = span[newIndex..].ReadVariableInteger(out newIndex, out embeddedValue);
        encoding ??= Encoding.UTF8;
        return span[newIndex..].Read(out newIndex, (int)length, data => encoding.GetString(data));
    }
}*/

/*public static class MemoryReaderExtensions
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

    public static BigInteger ReadVariableInteger(this IMemoryReader<byte> reader, out int embeddedValue)
    {
        int prefix = reader.Read();
        embeddedValue = prefix >> 5;
        int value = prefix & 0b00001111;

        if (!prefix.GetBit(4))
        {
            return value - 2;
        }
        else
        {
            value++;

            if (value == 16)
            {
                value += reader.Read();
            }

            return reader.Read<byte, BigInteger>(value, data => new BigInteger(data));
        }
    }

    public static string ReadString(this IMemoryReader<byte> reader, out int embeddedValue, Encoding? encoding = null)
    {
        BigInteger length = reader.ReadVariableInteger(out embeddedValue);
        encoding ??= Encoding.UTF8;
        return reader.Read<byte, string>((int)length, data => encoding.GetString(data));
    }
}*/
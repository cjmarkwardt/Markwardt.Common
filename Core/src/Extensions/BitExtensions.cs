using System.Runtime.InteropServices;

namespace Markwardt;

public static class BitExtensions
{
    public static IEnumerable<bool> GetBits<T>(this T value)
        where T : INumberBase<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
    {
        int bitCount = Marshal.SizeOf<T>() * 8;
        for (int i = 0; i < bitCount; i++)
        {
            yield return value.GetBit(i);
        }
    }

    public static IEnumerable<bool> GetBits(this ReadOnlyMemory<byte> value)
    {
        int bitCount = value.Length * 8;
        for (int i = 0; i < bitCount; i++)
        {
            yield return value.Span.GetBit(i);
        }
    }

    public static IEnumerable<bool> GetBits(this ReadOnlySpan<byte> value)
    {
        int bitCount = value.Length * 8;
        List<bool> bits = new(bitCount);
        for (int i = 0; i < bitCount; i++)
        {
            bits.Add(value.GetBit(i));
        }

        return bits;
    }

    public static IEnumerable<bool> GetBits(this IReadOnlyList<byte> value)
    {
        int bitCount = value.Count * 8;
        for (int i = 0; i < bitCount; i++)
        {
            yield return value.GetBit(i);
        }
    }

    public static bool GetBit<T>(this T value, int bit)
        where T : INumberBase<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
        => (value & (T.One << bit)) != T.Zero;

    public static bool GetBit(this ReadOnlySpan<byte> value, int bit)
        => (value[bit / 8] & (1 << (bit % 8))) != 0;

    public static bool GetBit(this IReadOnlyList<byte> value, int bit)
        => (value[bit / 8] & (1 << (bit % 8))) != 0;

    public static T SetBit<T>(this T value, int bit, bool set)
        where T : INumberBase<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
        => set ? value.SetBit(bit) : value.ClearBit(bit);

    public static void SetBit(this Span<byte> value, int bit, bool set)
    {
        if (set)
        {
            value.SetBit(bit);
        }
        else
        {
            value.ClearBit(bit);
        }
    }

    public static void SetBit(this IList<byte> value, int bit, bool set)
    {
        if (set)
        {
            value.SetBit(bit);
        }
        else
        {
            value.ClearBit(bit);
        }
    }

    public static T SetBit<T>(this T value, int bit)
        where T : INumberBase<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
        => value | (T.One << bit);

    public static void SetBit(this Span<byte> value, int bit)
        => value[bit / 8] |= (byte)(1 << (bit % 8));

    public static void SetBit(this IList<byte> value, int bit)
        => value[bit / 8] |= (byte)(1 << (bit % 8));

    public static T ClearBit<T>(this T value, int bit)
        where T : INumberBase<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
        => value & ~(T.One << bit);

    public static void ClearBit(this Span<byte> value, int bit)
        => value[bit / 8] &= (byte)~(1 << (bit % 8));

    public static void ClearBit(this IList<byte> value, int bit)
        => value[bit / 8] &= (byte)~(1 << (bit % 8));
}
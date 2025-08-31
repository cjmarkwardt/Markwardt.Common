namespace Markwardt;

public static class BitExtensions
{
    public static bool GetBit<T>(this T value, int bit)
        where T : INumberBase<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
        => (value & (T.One << bit)) != T.Zero;

    public static T SetBit<T>(this T value, int bit, bool set)
        where T : INumberBase<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
        => set ? value.SetBit(bit) : value.ClearBit(bit);

    public static T SetBit<T>(this T value, int bit)
        where T : INumberBase<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
        => value | (T.One << bit);

    public static T ClearBit<T>(this T value, int bit)
        where T : INumberBase<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
        => value & ~(T.One << bit);
}
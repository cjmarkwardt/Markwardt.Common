namespace Markwardt;

[Flags]
public enum NetworkConstraints : byte
{
    None = 0,

    /// <summary> Data will always arrive. </summary>
    Reliable = 0b1,

    /// <summary> Data will never arrive more than once. </summary>
    Distinct = 0b10,

    /// <summary> Data will arrive in the correct order. </summary>
    Ordered = 0b100,

    All = Reliable | Distinct | Ordered
}
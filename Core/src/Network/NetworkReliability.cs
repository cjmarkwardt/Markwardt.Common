namespace Markwardt;

public enum NetworkReliability
{
    /// <summary> Data may be lost, duplicated, or arrive out of order. </summary>
    Unreliable,

    /// <summary> Data will always arrive without duplicates, but may arrive out of order. </summary>
    Reliable,

    /// <summary> Data will always arrive in the correct order. </summary>
    Ordered
}
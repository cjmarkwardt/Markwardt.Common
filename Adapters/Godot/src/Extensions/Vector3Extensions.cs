namespace Markwardt;

public static class Vector3Extensions
{
    public static Vector3 SetX(this Vector3 value, Func<float, float> set)
        => new(set(value.X), value.Y, value.Z);

    public static Vector3 SetX(this Vector3 value, float x)
        => new(x, value.Y, value.Z);

    public static Vector3 SetY(this Vector3 value, Func<float, float> set)
        => new(value.X, set(value.Y), value.Z);

    public static Vector3 SetY(this Vector3 value, float y)
        => new(value.X, y, value.Z);

    public static Vector3 SetZ(this Vector3 value, Func<float, float> set)
        => new(value.X, value.Y, set(value.Z));

    public static Vector3 SetZ(this Vector3 value, float z)
        => new(value.X, value.Y, z);

    public static Vector2I GetColumn(this Vector3I value)
        => new(value.X, value.Z);

    public static Vector3I RoundToInt(this Vector3 value)
    {
        Vector3 rounded = value.Round();
        return new((int)rounded.X, (int)rounded.Y, (int)rounded.Z);
    }

    public static Vector3I FloorToInt(this Vector3 value)
    {
        Vector3 rounded = value.Floor();
        return new((int)rounded.X, (int)rounded.Y, (int)rounded.Z);
    }

    public static Vector3 ToVector3(this Vector3I value)
        => new(value.X, value.Y, value.Z);

    public static Vector3 Multiply(this Vector3 value, double scalar)
        => new((float)(value.X * scalar), (float)(value.Y * scalar), (float)(value.Z * scalar));

    public static Vector3 Damp(this Vector3 value, Vector3 target, float smoothing, float deltaTime, float cutoff = 0)
        => new(value.X.Damp(target.X, smoothing, deltaTime, cutoff), value.Y.Damp(target.Y, smoothing, deltaTime, cutoff), value.Y.Damp(target.Y, smoothing, deltaTime, cutoff));
}
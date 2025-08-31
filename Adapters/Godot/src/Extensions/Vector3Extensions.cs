namespace Markwardt;

public static class Vector3Extensions
{
    public static Vector2I GetColumn(this Vector3I value)
        => new(value.X, value.Z);

    public static Vector3I RoundToInt(this Vector3 value)
    {
        Vector3 rounded = value.Round();
        return new((int)rounded.X, (int)rounded.Y, (int)rounded.Z);
    }

    public static Vector3 Multiply(this Vector3 value, double scalar)
        => new((float)(value.X * scalar), (float)(value.Y * scalar), (float)(value.Z * scalar));

    public static Vector3 Damp(this Vector3 value, Vector3 target, float smoothing, float deltaTime, float cutoff = 0)
        => new(value.X.Damp(target.X, smoothing, deltaTime, cutoff), value.Y.Damp(target.Y, smoothing, deltaTime, cutoff), value.Y.Damp(target.Y, smoothing, deltaTime, cutoff));
}
namespace Markwardt;

public static class Vector2Extensions
{
    public static Vector3I GetBlock(this Vector2I value, int height)
        => new(value.X, height, value.Y);

    public static Vector2I RoundToInt(this Vector2 value)
    {
        Vector2 rounded = value.Round();
        return new((int)rounded.X, (int)rounded.Y);
    }
}
namespace Markwardt;

public static class GodotColorRectExtensions
{
    public static T WithColor<T>(this T control, Color color)
        where T : ColorRect
        => control.Do(_ => control.Color = color);
}
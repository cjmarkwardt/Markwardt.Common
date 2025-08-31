namespace Markwardt;

public static class GodotLineEditExtensions
{
    public static T OnTextSubmitted<T>(this T control, Action<string> action)
        where T : LineEdit
        => control.Do(_ => control.TextSubmitted += x => action(x));
}
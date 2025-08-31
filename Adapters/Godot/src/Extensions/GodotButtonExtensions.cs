namespace Markwardt;

public static class GodotButtonExtensions
{
    public static T OnPressed<T>(this T control, Action action)
        where T : BaseButton
        => control.Do(_ => control.Pressed += action);

    public static T WithText<T>(this T control, string text)
        where T : Button
        => control.Do(_ => control.Text = text);
}
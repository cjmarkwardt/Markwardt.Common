namespace Markwardt;

public static class GodotLabelExtensions
{
    public static T WithText<T>(this T control, string text)
        where T : Label
        => control.Do(_ => control.Text = text);
}
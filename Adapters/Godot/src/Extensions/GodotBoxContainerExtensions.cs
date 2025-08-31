namespace Markwardt;

public static class GodotBoxContainerExtensions
{
    public static T WithAlignment<T>(this T control, BoxContainer.AlignmentMode alignment)
        where T : BoxContainer
        => control.Do(_ => control.Alignment = alignment);

    public static T WithColorBackground<T>(this T control, Color color)
        where T : Control
        => control.Do(_ => new ColorRect().WithParent(control).WithColor(color).WithLayoutPreset(Control.LayoutPreset.FullRect));
}
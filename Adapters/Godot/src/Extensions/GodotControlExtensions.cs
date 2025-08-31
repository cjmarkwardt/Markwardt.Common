namespace Markwardt;

public static class GodotControlExtensions
{
    public static T WithSize<T>(this T control, Godot.Vector2 size)
        where T : Control
        => control.Do(_ => control.Size = size);

    public static T WithThemeOverride<T>(this T control, string name, int value)
        where T : Control
        => control.Do(_ => control.AddThemeConstantOverride(name, value));

    public static T WithThemeOverride<T>(this T control, string name, Color value)
        where T : Control
        => control.Do(_ => control.AddThemeColorOverride(name, value));

    public static T WithThemeOverride<T>(this T control, string name, Font value)
        where T : Control
        => control.Do(_ => control.AddThemeFontOverride(name, value));

    public static T WithThemeOverride<T>(this T control, string name, StyleBox value)
        where T : Control
        => control.Do(_ => control.AddThemeStyleboxOverride(name, value));

    public static T WithFontColor<T>(this T control, Color value)
        where T : Control
        => control.WithThemeOverride("font_color", value);

    public static T WithCenterParent<T>(this T control, Node parent)
        where T : Control
        => control.WithParent(new CenterContainer().WithParent(parent).WithLayoutPreset(Control.LayoutPreset.FullRect));

    public static T WithMarginParent<T>(this T control, Node parent, int left, int right, int top, int bottom)
        where T : Control
        => control.WithParent(new MarginContainer().WithParent(parent).WithLayoutPreset(Control.LayoutPreset.FullRect).WithMargins(left, right, top, bottom));

    public static T WithMarginParent<T>(this T control, Node parent, int horizontal, int vertical)
        where T : Control
        => control.WithMarginParent(parent, horizontal, horizontal, vertical, vertical);

    public static T WithMarginParent<T>(this T control, Node parent, int uniform)
        where T : Control
        => control.WithMarginParent(parent, uniform, uniform);

    public static T WithMargins<T>(this T control, int left, int right, int top, int bottom)
        where T : Control
        => control.WithThemeOverride("margin_left", left).WithThemeOverride("margin_right", right).WithThemeOverride("margin_top", top).WithThemeOverride("margin_bottom", bottom);

    public static T WithMargins<T>(this T control, int horizontal, int vertical)
        where T : Control
        => control.WithMargins(horizontal, horizontal, vertical, vertical);

    public static T WithMargins<T>(this T control, int uniform)
        where T : Control
        => control.WithMargins(uniform, uniform);

    public static T WithAnchors<T>(this T control, float left, float right, float top, float bottom)
        where T : Control
        => control.Do(_ =>
        {
            control.AnchorLeft = left;
            control.AnchorRight = right;
            control.AnchorTop = top;
            control.AnchorBottom = bottom;
        });

    public static T WithAnchors<T>(this T control, float horizontal, float vertical)
        where T : Control
        => control.WithAnchors(horizontal, horizontal, vertical, vertical);

    public static T WithAnchors<T>(this T control, float uniform)
        where T : Control
        => control.WithAnchors(uniform, uniform);

    public static T WithOffsets<T>(this T control, float left, float right, float top, float bottom)
        where T : Control
        => control.Do(_ =>
        {
            control.OffsetLeft = left;
            control.OffsetRight = right;
            control.OffsetTop = top;
            control.OffsetBottom = bottom;
        });

    public static T WithOffsets<T>(this T control, float horizontal, float vertical)
        where T : Control
        => control.WithOffsets(horizontal, horizontal, vertical, vertical);

    public static T WithOffsets<T>(this T control, float uniform)
        where T : Control
        => control.WithOffsets(uniform, uniform);

    public static T WithAnchorsPreset<T>(this T control, Control.LayoutPreset preset, bool keepOffsets = false)
        where T : Control
        => control.Do(_ => control.SetAnchorsPreset(preset, keepOffsets));

    public static T WithOffsetsPreset<T>(this T control, Control.LayoutPreset preset, Control.LayoutPresetMode resizeMode = Control.LayoutPresetMode.Minsize, int margin = 0)
        where T : Control
        => control.Do(_ => control.SetOffsetsPreset(preset, resizeMode, margin));

    public static T WithHorizontalSizeFlag<T>(this T control, Control.SizeFlags flag)
        where T : Control
        => control.Do(_ => control.SizeFlagsHorizontal = flag);

    public static T WithVerticalSizeFlag<T>(this T control, Control.SizeFlags flag)
        where T : Control
        => control.Do(_ => control.SizeFlagsVertical = flag);

    public static T WithLayoutPreset<T>(this T control, Control.LayoutPreset preset)
        where T : Control
        => control.WithAnchorsPreset(preset).WithOffsetsPreset(preset);
}
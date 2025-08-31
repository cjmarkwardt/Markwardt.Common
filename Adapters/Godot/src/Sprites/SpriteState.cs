namespace Markwardt;

public interface ISpriteState
{
    Color ResolveColor(object color);
    void OverrideColor(object color, Color value);
    void ClearColor(object color);

    Vector2 ResolveAnchor(object? anchor);
    void OverrideAnchor(object? anchor, Vector2 value);
    void ClearAnchor(object? anchor);

    AnimatorState ResolveAnimator(object? animator);
    void OverrideAnimator(object? animator, AnimatorState value);
    void ClearAnimator(object? animator);

    Texture2D ResolvePalette(object? palette);
}

public class SpriteState(ISpriteProfile profile, IEnumerable<Sprite> activeSprites) : ISpriteState
{
    private readonly ExtendedDictionary<object, Color> colors = [];
    private readonly ExtendedDictionary<object?, Vector2> anchors = [];
    private readonly ExtendedDictionary<object?, AnimatorState> animators = [];

    public Color ResolveColor(object color)
        => colors.GetValueOrDefault(color, default);

    public void OverrideColor(object color, Color value)
        => colors[color] = value;

    public void ClearColor(object color)
        => colors.Remove(color);

    public Vector2 ResolveAnchor(object? anchor)
    {
        GD.Print("----------");
        GD.Print(anchor);
        Vector2 value = anchors.GetValueOrDefault(anchor);
        GD.Print(value);

        GD.Print("// OVERRIDE START");
        foreach (Sprite sprite in activeSprites)
        {
            GD.Print(string.Join(", ", sprite.AnchorOverrides.Keys));
            if (sprite.AnchorOverrides.TryGetValue(anchor, out Vector2 spriteOverride))
            {
                GD.Print("OVERRIDE");
                value = spriteOverride;
            }
        }
        GD.Print("// OVERRIDE END");
        GD.Print(value);

        GD.Print("// PARENT START");
        if (anchor is not null && profile.TryGetAnchorParent(anchor, out object? parent))
        {
            value += ResolveAnchor(parent);
        }
        GD.Print("// PARENT END");
        GD.Print(value);

        return value;
    }

    public void OverrideAnchor(object? anchor, Vector2 value)
        => anchors[anchor] = value;

    public void ClearAnchor(object? anchor)
        => anchors.Remove(anchor);

    public AnimatorState ResolveAnimator(object? animator)
    {
        if (animator is not null && profile.TryGetAnchorParent(animator, out object? parent))
        {
            return ResolveAnimator(parent);
        }
        else
        {
            return animators.GetValueOrDefault(animator);
        }
    }

    public void OverrideAnimator(object? animator, AnimatorState value)
        => animators[animator] = value;

    public void ClearAnimator(object? animator)
        => animators.Remove(animator);

    public Texture2D ResolvePalette(object? palette)
    {
        if (!profile.TryGetPalette(palette, out IReadOnlyList<object>? paletteColors))
        {
            return EmptyTexture.Instance;
        }

        Image image = Image.CreateEmpty(paletteColors.Count, 1, false, Image.Format.Rgba8);
        int i = 0;
        foreach (object color in paletteColors)
        {
            image.SetPixel(i++, 0, ResolveColor(color));
        }

        return ImageTexture.CreateFromImage(image);
    }
}
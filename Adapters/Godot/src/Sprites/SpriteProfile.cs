namespace Markwardt;

public interface ISpriteProfile
{
    int GetLayerOrder(object? layer);
    int GetElementOrder(object element);
    IEnumerable<object> GetAnchorChildren(object? anchor);
    IEnumerable<object> GetAnimatorChildren(object? animator);
    IEnumerable<object?> FindPalettes(object color);
    bool TryGetPalette(object? palette, [NotNullWhen(true)] out IReadOnlyList<object>? colors);
    bool TryGetAnchorParent(object anchor, [NotNullWhen(true)] out object? parent);
    bool TryGetAnimatorParent(object animator, [NotNullWhen(true)] out object? parent);
}

public class SpriteProfile : ISpriteProfile
{
    public SpriteProfile
    (
        IEnumerable<object> layers,
        IEnumerable<object> elements,
        IEnumerable<(object? Palette, IEnumerable<object> Colors)> palettes,
        IEnumerable<object> anchors,
        IEnumerable<(object Anchor, object Parent)> anchorParents,
        IEnumerable<object> animators,
        IEnumerable<(object Animator, object Parent)> animatorParents
    )
    {
        this.layers = layers.Select((layer, index) => (layer, index)).ToExtendedDictionary(x => x.layer, x => x.index);
        this.elements = elements.Select((element, index) => (element, index)).ToExtendedDictionary(x => x.element, x => x.index);
        this.palettes = palettes.ToExtendedDictionary(x => x.Palette, x => (IReadOnlyList<object>)x.Colors.ToList());
        colorToPalettes = palettes.SelectMany(x => x.Colors, (x, color) => (x.Palette, color)).GroupBy(x => x.color, x => x.Palette).ToExtendedDictionary(x => x.Key, x => (IReadOnlyList<object?>)x.ToList());
        this.anchorParents = anchorParents.ToExtendedDictionary(x => x.Anchor, x => x.Parent);
        anchorChildren = anchorParents.GroupBy(x => x.Parent).ToExtendedDictionary(x => x.Key, x => (IReadOnlyList<object>)x.Select(y => y.Anchor).ToList());
        this.animatorParents = animatorParents.ToExtendedDictionary(x => x.Animator, x => x.Parent);
        animatorChildren = anchorParents.GroupBy(x => x.Parent).ToExtendedDictionary(x => x.Key, x => (IReadOnlyList<object>)x.Select(y => y.Anchor).ToList());

        defaultAnchorChildren = anchors.Where(x => !this.anchorParents.ContainsKey(x)).ToList();
        defaultAnimatorChildren = animators.Where(x => !this.animatorParents.ContainsKey(x)).ToList();
    }

    private readonly IReadOnlyDictionary<object, int> layers;
    private readonly IReadOnlyDictionary<object, int> elements;
    private readonly IReadOnlyDictionary<object?, IReadOnlyList<object>> palettes;
    private readonly IReadOnlyDictionary<object, IReadOnlyList<object?>> colorToPalettes;
    private readonly IReadOnlyDictionary<object, object> anchorParents;
    private readonly IReadOnlyList<object> defaultAnchorChildren;
    private readonly IReadOnlyDictionary<object, IReadOnlyList<object>> anchorChildren;
    private readonly IReadOnlyDictionary<object, object> animatorParents;
    private readonly IReadOnlyList<object> defaultAnimatorChildren;
    private readonly IReadOnlyDictionary<object, IReadOnlyList<object>> animatorChildren;

    public int GetLayerOrder(object? layer)
        => layer is null ? 0 : layers.GetValueOrDefault(layer);

    public int GetElementOrder(object element)
        => elements.GetValueOrDefault(element);

    public IEnumerable<object> GetAnchorChildren(object? anchor)
    {
        if (anchor is null)
        {
            return defaultAnchorChildren;
        }
        else if (anchorChildren.TryGetValue(anchor, out IReadOnlyList<object>? children))
        {
            return children;
        }
        else
        {
            return [];
        }
    }

    public IEnumerable<object> GetAnimatorChildren(object? animator)
    {
        if (animator is null)
        {
            return defaultAnimatorChildren;
        }
        else if (animatorChildren.TryGetValue(animator, out IReadOnlyList<object>? children))
        {
            return children;
        }
        else
        {
            return [];
        }
    }

    public IEnumerable<object?> FindPalettes(object color)
    {
        if (color is object casted && colorToPalettes.TryGetValue(casted, out IReadOnlyList<object?>? palettes))
        {
            foreach (object? palette in palettes)
            {
                yield return palette;
            }
        }
    }

    public bool TryGetPalette(object? palette, [NotNullWhen(true)] out IReadOnlyList<object>? colors)
        => palettes.TryGetValue(palette, out colors);

    public bool TryGetAnchorParent(object anchor, [NotNullWhen(true)] out object? parent)
        => anchorParents.TryGetValue(anchor, out parent);

    public bool TryGetAnimatorParent(object animator, [NotNullWhen(true)] out object? parent)
        => animatorParents.TryGetValue(animator, out parent);
}

public class SpriteProfile<TLayer, TElement, TAnchor, TAnimator>(IEnumerable<(object? Palette, IEnumerable<object> Colors)> palettes, IEnumerable<(object Anchor, object Parent)> anchorParents, IEnumerable<(object Animator, object Parent)> animatorParents)
    : SpriteProfile(Enum.GetValues<TLayer>().Cast<object>(), Enum.GetValues<TElement>().Cast<object>(), palettes, Enum.GetValues<TAnchor>().Cast<object>(), anchorParents, Enum.GetValues<TAnimator>().Cast<object>(), animatorParents), ISpriteProfile
    where TLayer : struct, Enum
    where TElement : struct, Enum
    where TAnchor : struct, Enum
    where TAnimator : struct, Enum;
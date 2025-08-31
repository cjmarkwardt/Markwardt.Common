namespace Markwardt;

public interface ISpritePresenter
{
    IEnumerable<object> Elements { get; }

    bool Flip { get; set; }

    void SetElements(params IEnumerable<(object Element, SpriteComposite? Sprite)> elements);
    void SetColors(params IEnumerable<(object Color, Color Value)> colors);
    void SetAnchors(params IEnumerable<(object? Anchor, Vector2 Value)> anchors);
    void SetAnimators(params IEnumerable<(object? Animator, object? Animation, bool Replay)> animators);
    void ResetAnimators(params IEnumerable<object?> animators);
}

public static class SpritePresenterExtensions
{
    public static void SetElement(this ISpritePresenter presenter, object element, SpriteComposite? sprite)
        => presenter.SetElements((element, sprite).Yield());

    public static void ClearElements(this ISpritePresenter presenter)
        => presenter.SetElements(presenter.Elements.Select(x => (x, (SpriteComposite?)null)).ToList());

    public static void SetColor(this ISpritePresenter presenter, object color, Color value)
        => presenter.SetColors((color, value).Yield());

    public static void SetAnchor(this ISpritePresenter presenter, object? anchor, Vector2 value)
        => presenter.SetAnchors((anchor, value).Yield());

    public static void SetAnimator(this ISpritePresenter presenter, object? animator, object? animation, bool replay = false)
        => presenter.SetAnimators((animator, animation, replay).Yield());

    public static void SetAnimator(this ISpritePresenter presenter, object? animation, bool replay = false)
        => presenter.SetAnimator(default, animation, replay);
}

public partial class SpritePresenter : Node3D, ISpritePresenter
{
    public delegate SpritePresenter Factory(ISpriteProfile? profile = null);

    public SpritePresenter(ISpriteProfile? profile = null)
    {
        this.profile = profile ?? new DefaultSpriteProfile();

        state = new(this.profile, Sprites.Where(x => x.IsEnabled).Select(x => x.Sprite));
    }

    private readonly ISpriteProfile profile;
    private readonly SpriteState state;
    private readonly NodeDisposer disposer = new();
    private readonly ExtendedDictionary<object, Element> elements = [];

    private IEnumerable<SpriteInstance> Sprites => elements.Values.SelectMany(x => x.Instances);

    public IEnumerable<object> Elements => elements.Keys;

    private bool flip;
    public bool Flip { get => flip; set => Field.TrySet(ref flip, value, _ => Sprites.ForEach(x => x.SetFlip(value))); }

    public override void _Notification(int what)
    {
        base._Notification(what);
        disposer.ReadNotification(what);
    }

    public void SetElements(params IEnumerable<(object Element, SpriteComposite? Sprite)> elements)
    {
        foreach ((object element, SpriteComposite? sprite) in elements)
        {
            Element? elementObj = this.elements.GetValueOrDefault(element);

            if (elementObj?.Sprite == sprite)
            {
                continue;
            }

            if (elementObj is not null)
            {
                this.elements.Remove(elementObj);
                elementObj.Dispose();
            }

            if (sprite is not null)
            {
                elementObj = new(this, sprite, element);
                this.elements.Add(element, elementObj);
            }
        }

        ExtendedDictionary<object, SpriteInstance> layerToHighestPriority = [];
        foreach (SpriteInstance sprite in Sprites)
        {
            sprite.IsEnabled = false;

            SpriteInstance? highest = layerToHighestPriority.GetValueOrDefault(sprite.Layer);
            if (highest is null || sprite.Priority > highest.Priority)
            {
                layerToHighestPriority[sprite.Layer] = sprite;
            }
        }

        layerToHighestPriority.Values.ForEach(x =>
        {
            x.IsEnabled = true;
            x.SetFlip(Flip);
            x.SetOffset(state.ResolveAnchor(x.Sprite.Anchor));
            x.SetColors(state.ResolvePalette(x.Sprite.Palette));
            x.SetAnimator(state.ResolveAnimator(x.Sprite.Animator));
        });
    }

    public void SetColors(params IEnumerable<(object Color, Color Value)> colors)
    {
        if (colors.Any())
        {
            colors.ForEach(x => state.OverrideColor(x.Color, x.Value));

            ExtendedDictionary<object?, Texture2D> paletteTextures = colors.Select(x => x.Color).SelectMany(profile.FindPalettes).Distinct().ToExtendedDictionary(x => x, state.ResolvePalette);
            foreach (SpriteInstance sprite in Sprites)
            {
                if (paletteTextures.TryGetValue(sprite.Sprite.Palette, out Texture2D? texture))
                {
                    sprite.SetColors(texture);
                }
            }
        }
    }

    public void SetAnchors(params IEnumerable<(object? Anchor, Vector2 Value)> anchors)
    {
        anchors.ForEach(x => state.OverrideAnchor(x.Anchor, x.Value));

        foreach (object? anchor in anchors.Select(x => x.Anchor).SelectMany(x => profile.GetAnchorChildren(x).Prepend(x)).Distinct())
        {
            Sprites.Where(x => x.Sprite.Anchor == anchor).ForEach(x => x.SetOffset(state.ResolveAnchor(anchor)));
        }
    }

    public void SetAnimators(params IEnumerable<(object? Animator, object? Animation, bool Replay)> animators)
    {
        foreach ((object? animator, object? animation, bool replay) in animators)
        {
            AnimatorState value;

            if (replay)
            {
                value = new(animation, Time.Singleton.GetElapsed());
            }
            else
            {
                value = state.ResolveAnimator(animator);

                if (value.Animation != animation)
                {
                    value = new(animation, Time.Singleton.GetElapsed());
                }
            }

            state.OverrideAnimator(animator, value);
        }

        foreach (object? animator in animators.Select(x => x.Animator).SelectMany(x => profile.GetAnimatorChildren(x).Prepend(x)).Distinct())
        {
            Sprites.Where(x => x.Sprite.Animator == animator).ForEach(x => x.SetAnimator(state.ResolveAnimator(animator)));
        }
    }

    public void ResetAnimators(params IEnumerable<object?> animators)
    {
        animators.ForEach(state.ClearAnimator);

        foreach (object? animator in animators.SelectMany(x => profile.GetAnimatorChildren(x).Prepend(x)).Distinct())
        {
            Sprites.Where(x => x.Sprite.Animator == animator).ForEach(x => x.SetAnimator(state.ResolveAnimator(animator)));
        }
    }

    private class Element : BaseDisposable
    {
        public Element(SpritePresenter presenter, SpriteComposite sprite, object element)
        {
            Sprite = sprite;
            Instances = sprite.Sprites.Select(x => new SpriteInstance(x, presenter.profile.GetLayerOrder(x.Layer), presenter.profile.GetElementOrder(element)).DisposeWith(this)).ToList();
            Instances.ForEach(x => presenter.AddChild(x));
        }

        public SpriteComposite Sprite { get; }
        public IReadOnlyList<SpriteInstance> Instances { get; }

        public void Edit(Action<ISpriteInstance> edit)
            => Instances.ForEach(x => edit(x));
    }
}
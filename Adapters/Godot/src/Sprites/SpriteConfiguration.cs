namespace Markwardt;

public interface ISpriteConfiguration
{
    SpriteComposite CreateSprite(IEnumerable<(object Layer, SpriteAtlas Atlas)> atlases, IEnumerable<(object Layer, object? Anchor, Vector2 Value)> anchorOverrides);
    SpritePresenter CreatePresenter();
}

public abstract class SpriteConfiguration<TLayer, TElement, TAnchor, TAnimator> : ISpriteConfiguration
    where TLayer : struct, Enum
    where TElement : struct, Enum
    where TAnchor : struct, Enum
    where TAnimator : struct, Enum
{
    public SpriteConfiguration()
    {
        profile = new(GetPalettes(), GetAnchorParents(), GetAnimatorParents());

        builder = new();
        ConfigureBuilder(builder);
    }

    private readonly SpriteProfile<TLayer, TElement, TAnchor, TAnimator> profile;
    private readonly SpriteCompositeBuilder builder;

    public required SpritePresenter.Factory PresenterFactory { get; init; }

    public SpriteComposite CreateSprite(IEnumerable<(object Layer, SpriteAtlas Atlas)> atlases, IEnumerable<(object Layer, object? Anchor, Vector2 Value)> anchorOverrides)
        => builder.Build(atlases, anchorOverrides);

    public SpritePresenter CreatePresenter()
        => PresenterFactory(profile);

    protected virtual IEnumerable<(object? Palette, IEnumerable<object> Colors)> GetPalettes()
        => [];

    protected virtual IEnumerable<(object Anchor, object Parent)> GetAnchorParents()
        => [];

    protected virtual IEnumerable<(object Animator, object Parent)> GetAnimatorParents()
        => [];

    protected abstract void ConfigureBuilder(ISpriteCompositeBuilder builder);
}
namespace Markwardt;

public interface ISpriteAnimationBuilder : ISpriteLayerBuilder
{
    ISpriteAnimationBuilder FrameRate(float value);
    ISpriteAnimationBuilder Frames(IReadOnlyList<SpriteFrame> frames);
}

public static class SpriteAnimationBuilderExtensions
{
    public static ISpriteAnimationBuilder Frame(this ISpriteAnimationBuilder builder, int image, Vector2 offset = default)
        => builder.Frames(SpriteFrame.FromImage(image, offset));

    public static ISpriteAnimationBuilder Frames(this ISpriteAnimationBuilder builder, IEnumerable<int?> images, Vector2 offset = default)
        => builder.Frames(SpriteFrame.FromImages(images, offset));

    public static ISpriteAnimationBuilder Frames(this ISpriteAnimationBuilder builder, int start, int length, Vector2 offset = default)
        => builder.Frames(SpriteFrame.FromImages(start, length, offset));

    public static ISpriteAnimationBuilder Frames(this ISpriteAnimationBuilder builder, int image, IEnumerable<Vector2> offsets, Vector2 baseOffset = default)
        => builder.Frames(SpriteFrame.FromOffsets(image, offsets, baseOffset));
}

public interface ISpriteLayerBuilder : ISpriteCompositeBuilder
{
    ISpriteLayerBuilder Palette(object? palette);
    ISpriteLayerBuilder Anchor(object? anchor);
    ISpriteLayerBuilder Animator(object? animator);
    ISpriteAnimationBuilder Animation(object? animation);
}

public interface ISpriteCompositeBuilder
{
    ISpriteCompositeBuilder DefaultFrameRate(object? animation, float value);
    ISpriteLayerBuilder Layer(object layer);
    SpriteComposite Build(IEnumerable<(object Layer, SpriteAtlas Atlas)> atlases, IEnumerable<(object Layer, object? Anchor, Vector2 Value)> anchorOverrides);
}

public class SpriteCompositeBuilder : ISpriteCompositeBuilder
{
    private readonly ExtendedDictionary<object?, float> defaultFrameRates = [];
    private readonly ExtendedDictionary<object, LayerBuilder> layers = [];

    public ISpriteCompositeBuilder DefaultFrameRate(object? animation, float value)
    {
        defaultFrameRates[animation] = value;
        return this;
    }

    public ISpriteLayerBuilder Layer(object layer)
    {
        if (!layers.TryGetValue(layer, out LayerBuilder? builder))
        {
            builder = new(this, layer);
            layers.Add(layer, builder);
        }

        return builder;
    }

    public SpriteComposite Build(IEnumerable<(object Layer, SpriteAtlas Atlas)> atlases, IEnumerable<(object Layer, object? Anchor, Vector2 Value)> anchorOverrides)
    {
        IReadOnlyDictionary<object, IReadOnlyDictionary<object?, Vector2>> layerAnchors = anchorOverrides.GroupBy(x => x.Layer).ToExtendedDictionary(x => x.Key, x => (IReadOnlyDictionary<object?, Vector2>)x.ToExtendedDictionary(y => y.Anchor, y => y.Value));

        List<Sprite> sprites = [];
        foreach ((object layer, SpriteAtlas atlas) in atlases)
        {
            IReadOnlyDictionary<object?, Vector2> anchors = layerAnchors.GetValueOrDefault(layer, EmptyDictionary<object?, Vector2>.Instance);

            if (layers.TryGetValue(layer, out LayerBuilder? layerBuilder))
            {
                ExtendedDictionary<object?, SpriteAnimation> animations = [];
                foreach ((object? animation, AnimationBuilder animationBuilder) in layerBuilder.Animations)
                {
                    float? frameRate = animationBuilder.TargetFrameRate;
                    if (frameRate is null && defaultFrameRates.TryGetValue(animation, out float defaultFrameRate))
                    {
                        frameRate = defaultFrameRate;
                    }

                    if (frameRate is not null && animationBuilder.TargetFrames is not null)
                    {
                        animations.Add(animation, new(atlas, frameRate.Value, animationBuilder.TargetFrames));
                    }
                }

                sprites.Add(new()
                {
                    Layer = layer,
                    Palette = layerBuilder.TargetPalette,
                    Anchor = layerBuilder.TargetAnchor,
                    Animator = layerBuilder.TargetAnimator,
                    AnchorOverrides = anchors,
                    Animations = animations
                });
            }
        }

        return new(sprites);
    }

    private class LayerBuilder(SpriteCompositeBuilder builder, object layer) : ISpriteLayerBuilder
    {
        private readonly ExtendedDictionary<object?, AnimationBuilder> animations = [];
        private readonly ExtendedDictionary<object?, Vector2> anchorOverrides = [];

        public object TargetLayer { get; } = layer;

        public SpriteAtlas? TargetAtlas { get; private set; }
        public object? TargetPalette { get; private set; }
        public object? TargetAnchor { get; private set; }
        public object? TargetAnimator { get; private set; }

        public IReadOnlyDictionary<object?, AnimationBuilder> Animations => animations;
        public IReadOnlyDictionary<object?, Vector2> AnchorOverrides => anchorOverrides;

        public ISpriteLayerBuilder Anchor(object? anchor)
        {
            TargetAnchor = anchor;
            return this;
        }

        public ISpriteAnimationBuilder Animation(object? animation)
        {
            if (!animations.TryGetValue(animation, out AnimationBuilder? builder))
            {
                builder = new(this, animation);
                animations.Add(animation, builder);
            }

            return builder;
        }

        public ISpriteLayerBuilder Animator(object? animator)
        {
            TargetAnimator = animator;
            return this;
        }

        public ISpriteLayerBuilder Atlas(SpriteAtlas atlas)
        {
            TargetAtlas = atlas;
            return this;
        }

        public ISpriteLayerBuilder Palette(object? palette)
        {
            TargetPalette = palette;
            return this;
        }

        public SpriteComposite Build(IEnumerable<(object Layer, SpriteAtlas Atlas)> atlases, IEnumerable<(object Layer, object? Anchor, Vector2 Value)> anchorOverrides)
            => builder.Build(atlases, anchorOverrides);

        public ISpriteCompositeBuilder DefaultFrameRate(object? animation, float value)
            => builder.DefaultFrameRate(animation, value);

        public ISpriteLayerBuilder Layer(object layer)
            => builder.Layer(layer);
    }

    private class AnimationBuilder(LayerBuilder builder, object? animation) : ISpriteAnimationBuilder
    {
        public object? TargetAnimation { get; } = animation;

        public float? TargetFrameRate { get; private set; }
        public IReadOnlyList<SpriteFrame>? TargetFrames { get; private set; }

        public ISpriteAnimationBuilder FrameRate(float value)
        {
            TargetFrameRate = value;
            return this;
        }

        public ISpriteAnimationBuilder Frames(IReadOnlyList<SpriteFrame> frames)
        {
            TargetFrames = frames;
            return this;
        }

        public ISpriteLayerBuilder Anchor(object? anchor)
            => builder.Anchor(anchor);

        public ISpriteAnimationBuilder Animation(object? animation)
            => builder.Animation(animation);

        public ISpriteLayerBuilder Animator(object? animator)
            => builder.Animator(animator);

        public ISpriteLayerBuilder Atlas(SpriteAtlas atlas)
            => builder.Atlas(atlas);

        public SpriteComposite Build(IEnumerable<(object Layer, SpriteAtlas Atlas)> atlases, IEnumerable<(object Layer, object? Anchor, Vector2 Value)> anchorOverrides)
            => builder.Build(atlases, anchorOverrides);

        public ISpriteCompositeBuilder DefaultFrameRate(object? animation, float value)
            => builder.DefaultFrameRate(animation, value);

        public ISpriteLayerBuilder Layer(object layer)
            => builder.Layer(layer);

        public ISpriteLayerBuilder Palette(object? palette)
            => builder.Palette(palette);
    }
}
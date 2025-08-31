namespace Markwardt;

public record struct AnimatorState(object? Animation, float Start);

public interface ISpriteInstance
{
    Sprite Sprite { get; }
    int Layer { get; }
    int Priority { get; }

    bool IsEnabled { get; set; }

    void SetFlip(bool value);
    void SetOffset(Vector2 value);
    void SetColors(Texture2D value);
    void SetAnimator(AnimatorState value);
}

public partial class SpriteInstance : MeshInstance3D, ISpriteInstance
{
    public delegate SpriteInstance Factory(Sprite sprite, int layer = 0, int priority = 0);

    private static readonly Lazy<Mesh> quad = new(() =>
    {
        using MeshGenerator generator = new();
        generator.Anchor = new(-0.5f, 0, 0);
        generator.AddQuad(new Triangle3(Vector3.Up, Vector3.Up + Vector3.Right, Vector3.Right).Quad, new Triangle2(Vector2.Up, Vector2.Up + Vector2.Right, Vector2.Right).Quad);
        return generator.Generate();
    });

    public SpriteInstance(Sprite sprite, int layer = 0, int priority = 0)
    {
        Sprite = sprite;
        Layer = layer;
        Priority = priority;

        Mesh = quad.Value;
        MaterialOverride = material;

        material.DisposeWith(disposer);
        atlasTexture.DisposeWith(disposer);

        atlasTexture.Loaded.Subscribe(x => OnAtlasLoaded(x.Value, x.Tag)).DisposeWith(disposer);
        atlasTexture.Unloaded.Subscribe(_ => OnAtlasUnloaded()).DisposeWith(disposer);
    }

    private readonly NodeDisposer disposer = new();
    private readonly SpriteMaterial material = new();
    private readonly AssetSpace<Texture2D, SpriteAtlas> atlasTexture = new();

    private bool flip;
    private Vector2 offset;
    private AnimatorState? animator;

    private SpriteAnimation? currentAnimation;
    private SpriteFrame? currentFrame;
    private int? currentImage;
    private Vector2 size;
    private float? pixelUnit;

    public Sprite Sprite { get; }
    public int Layer { get; }
    public int Priority { get; }

    private bool isEnabled;
    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            isEnabled = value;
            Visible = value;
            SetProcess(isEnabled);
        }
    }

    public void SetFlip(bool flip)
    {
        this.flip = flip;
        material.SetFlipX(flip);
        RefreshTransform();
    }

    public void SetOffset(Vector2 offset)
    {
        this.offset = offset;
        RefreshTransform();
    }

    public void SetColors(Texture2D colors)
        => material.SetColors(colors);

    public void SetAnimator(AnimatorState value)
    {
        object? oldAnimation = animator?.Animation;
        animator = value;

        if (oldAnimation is null || animator.Value.Animation != oldAnimation)
        {
            currentAnimation = Sprite.Animations.GetValueOrDefault(animator.Value.Animation);

            if (currentAnimation is not null)
            {
                atlasTexture.Load(currentAnimation.Atlas.Texture, currentAnimation.Atlas);
            }
            else
            {
                atlasTexture.Unload();
            }
        }
    }

    public override void _Process(double delta)
    {
        if (animator is not null && currentAnimation is not null)
        {
            SpriteFrame frame = currentAnimation.GetCurrentFrame(animator.Value.Start);
            if (currentFrame != frame)
            {
                currentFrame = frame;

                if (currentImage != currentFrame.Value.Image)
                {
                    currentImage = currentFrame.Value.Image;
                    material.SetImage(currentImage);
                }

                RefreshTransform();
            }
        }
    }

    private void OnAtlasLoaded(Texture2D texture, SpriteAtlas atlas)
    {
        size = texture.GetSize();
        size = new(size.X / atlas.Count, size.Y);
        pixelUnit = atlas.PixelUnit;
        material.SetAtlas(texture, atlas.Count);
        RefreshTransform();
    }

    private void OnAtlasUnloaded()
    {
        size = Vector2.Zero;
        pixelUnit = null;
        material.ClearAtlas();
    }

    private void RefreshTransform()
    {
        if (currentFrame is not null)
        {
            Vector2 offset = this.offset + currentFrame.Value.Offset;
            Vector2 scale = Vector2.One;

            if (flip)
            {
                offset = new(-offset.X, offset.Y);
            }

            if (pixelUnit is not null)
            {
                offset *= pixelUnit.Value;
                scale *= size * pixelUnit.Value;
            }

            Position = new(offset.X, offset.Y, 0.00001f * Layer);
            Scale = new(scale.X, scale.Y, 1);
        }
    }
}
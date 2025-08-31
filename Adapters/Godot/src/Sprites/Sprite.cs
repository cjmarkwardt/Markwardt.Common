namespace Markwardt;

public record Sprite
{
    public object? Layer { get; init; }
    public object? Palette { get; init; }
    public object? Anchor { get; init; }
    public object? Animator { get; init; }
    public IReadOnlyDictionary<object?, Vector2> AnchorOverrides { get; init; } = EmptyDictionary<object?, Vector2>.Instance;
    public IReadOnlyDictionary<object?, SpriteAnimation> Animations { get; init; } = EmptyDictionary<object?, SpriteAnimation>.Instance;
}
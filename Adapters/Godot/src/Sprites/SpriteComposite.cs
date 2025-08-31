namespace Markwardt;

public record SpriteComposite(IEnumerable<Sprite> Sprites)
{
    public static implicit operator SpriteComposite(Sprite sprite)
        => new(sprite.Yield());
}
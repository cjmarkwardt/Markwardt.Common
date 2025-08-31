namespace Markwardt;

public record SpriteAtlas(IAsset<Texture2D> Texture, int Count = 1)
{
    public float? PixelUnit { get; init; }
}
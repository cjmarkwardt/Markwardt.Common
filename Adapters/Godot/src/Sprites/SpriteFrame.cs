namespace Markwardt;

public record struct SpriteFrame(int? Image = null, Vector2 Offset = default)
{
    public static IReadOnlyList<SpriteFrame> FromImage(int image, Vector2 offset = default)
        => [new SpriteFrame(image, offset)];

    public static IReadOnlyList<SpriteFrame> FromImages(IEnumerable<int?> images, Vector2 offset = default)
        => images.Select(x => new SpriteFrame(x, offset)).ToList();

    public static IReadOnlyList<SpriteFrame> FromImages(int start, int length, Vector2 offset = default)
        => FromImages(Enumerable.Range(start, length).Select(x => (int?)x), offset);

    public static IReadOnlyList<SpriteFrame> FromOffsets(int image, IEnumerable<Vector2> offsets, Vector2 baseOffset = default)
        => offsets.Select(x => new SpriteFrame(image, x + baseOffset)).ToList();
}
namespace Markwardt;

public class DefaultSpriteProfile : ISpriteProfile
{
    public IEnumerable<object?> FindPalettes(object color)
        => [];

    public IEnumerable<object> GetAnchorChildren(object? anchor)
        => [];

    public IEnumerable<object> GetAnimatorChildren(object? animator)
        => [];

    public int GetElementOrder(object element)
        => 0;

    public int GetLayerOrder(object? layer)
        => 0;

    public bool TryGetAnchorParent(object anchor, [NotNullWhen(true)] out object? parent)
    {
        parent = default;
        return false;
    }

    public bool TryGetAnimatorParent(object animator, [NotNullWhen(true)] out object? parent)
    {
        parent = default;
        return false;
    }

    public bool TryGetPalette(object? palette, [NotNullWhen(true)] out IReadOnlyList<object>? colors)
    {
        colors = default;
        return false;
    }
}
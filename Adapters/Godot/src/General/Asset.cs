namespace Markwardt;

public interface IAsset<T>
{
    ValueTask<T> Load(CancellationToken cancellation);
}

public static class AssetExtensions
{
    public static IDisposable Reserve<T>(this IAsset<T> asset)
        => new AssetReservation<T>(asset);

    public static void LoadInto<T>(this IAsset<T>? asset, IAssetSpace<T> space, bool cache = false, bool unloadIfNull = false)
    {
        if (asset is not null)
        {
            space.Load(asset, cache);
        }
        else if (unloadIfNull)
        {
            space.Unload();
        }
    }

    public static void LoadInto<T, TTag>(this IAsset<T>? asset, IAssetSpace<T, TTag> space, TTag tag, bool cache = false, bool unloadIfNull = false)
    {
        if (asset is not null)
        {
            space.Load(asset, tag, cache);
        }
        else if (unloadIfNull)
        {
            space.Unload();
        }
    }
}

public static class EmptyTexture
{
    public static Texture2D Instance { get; } = ImageTexture.CreateFromImage(Image.CreateEmpty(1, 1, false, Image.Format.Rgba8));
}
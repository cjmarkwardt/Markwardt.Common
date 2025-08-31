namespace Markwardt;

public interface IAssetSpace<T> : IDisposable
{
    IObservable<T> Loaded { get; }
    IObservable Unloaded { get; }

    void Load(IAsset<T> asset, bool cache = false);
    void Unload();
    void ReleaseCache();
}

public interface IAssetSpace<T, TTag> : IDisposable
{
    IObservable<(T Value, TTag Tag)> Loaded { get; }
    IObservable Unloaded { get; }

    void Load(IAsset<T> asset, TTag tag, bool cache = false);
    void Unload();
    void ReleaseCache();
}

public static class AssetSpaceExtensions
{
    public static void LoadOrUnload<T>(this IAssetSpace<T> space, IAsset<T>? asset, bool cache = false)
    {
        if (asset is not null)
        {
            space.Load(asset, cache);
        }
        else
        {
            space.Unload();
        }
    }

    public static void LoadOrUnload<T, TTag>(this IAssetSpace<T, TTag> space, IAsset<T>? asset, TTag tag, bool cache = false)
    {
        if (asset is not null)
        {
            space.Load(asset, tag, cache);
        }
        else
        {
            space.Unload();
        }
    }
}

public class AssetSpace<T> : BaseDisposable, IAssetSpace<T>
{
    private readonly Dictionary<object, IDisposable> cache = [];

    private CancellationTokenSource? loadCancellation;
    private IAsset<T>? currentAsset;
    private IAsset<T>? loadingAsset;

    private readonly Subject<T> loaded = new();
    public IObservable<T> Loaded => loaded;

    private readonly Subject unloaded = new();
    public IObservable Unloaded => unloaded;

    public async void Load(IAsset<T> asset, bool cache = false)
    {
        this.VerifyUndisposed();

        if (asset is null)
        {
            ExecuteUnload(true);
        }
        else if (asset == currentAsset)
        {
            CancelLoad();
        }
        else if (asset != loadingAsset)
        {
            loadingAsset = asset;

            if (cache)
            {
                this.cache.TryAdd(asset, asset.Reserve);
            }

            CancellationTokenSource cancellation = new();
            CancelLoad(cancellation);

            T value;
            try
            {
                value = await asset.Load(cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            currentAsset = asset;
            loadingAsset = null;

            loaded.OnNext(value);
        }
    }

    public void Unload()
    {
        this.VerifyUndisposed();
        ExecuteUnload(true);
    }

    public void ReleaseCache()
    {
        this.VerifyUndisposed();
        ExecuteReleaseCache();
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        ExecuteUnload(false);
        ExecuteReleaseCache();
    }

    private void ExecuteUnload(bool notify)
    {
        if (currentAsset is not null)
        {
            currentAsset = null;
            loadingAsset = null;

            CancelLoad();

            if (notify)
            {
                unloaded.OnNext();
            }
        }
    }

    private void ExecuteReleaseCache()
    {
        foreach (IDisposable disposable in cache.Values)
        {
            disposable.Dispose();
        }

        cache.Clear();
    }

    private void CancelLoad(CancellationTokenSource? newLoadCancellation = null)
        => Field.Set(ref loadCancellation, newLoadCancellation)?.CancelAndDispose();
}

public class AssetSpace<T, TTag> : BaseDisposable, IAssetSpace<T, TTag>
{
    private readonly AssetSpace<T> space = new();

    private TTag? tag;

    public IObservable<(T Value, TTag Tag)> Loaded => space.Loaded.Select(x => (x, tag!));
    public IObservable Unloaded => space.Unloaded;

    public void Load(IAsset<T> asset, TTag tag, bool cache = false)
    {
        this.tag = tag;
        space.Load(asset, cache);
    }

    public void Unload()
        => space.Unload();

    public void ReleaseCache()
        => space.ReleaseCache();
}
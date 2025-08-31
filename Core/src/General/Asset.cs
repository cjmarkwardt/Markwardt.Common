namespace Markwardt;

public interface IAsset
{
    bool IsLoaded { get; }
    int Claims { get; }

    TimeSpan? UnloadDelay { get; set; }
}

public interface IAssetx<T> : IAsset
{
    ValueTask<IDisposable<T>> Load();
}

public abstract class Asset<T> : IAssetx<T>
{
    private Task<T>? currentLoad;
    private CancellationTokenSource? delayUnloadCancellation;

    public bool IsLoaded => Claims > 0;

    public int Claims { get; private set; }

    public TimeSpan? UnloadDelay { get; set; }

    public async ValueTask<IDisposable<T>> Load()
    {
        Claims++;
        delayUnloadCancellation?.Cancel();
        
        currentLoad ??= ExecuteLoad();
        T value = await currentLoad;

        bool isClaimed = true;
        return new Disposable<T>(value, () =>
        {
            if (isClaimed)
            {
                isClaimed = false;
                Claims--;

                if (Claims == 0)
                {
                    Unclaim(value);
                }
            }
        });
    }

    protected abstract Task<T> ExecuteLoad();
    protected virtual void ExecuteUnload(T value) { }

    private async void Unclaim(T value)
    {
        if (UnloadDelay is not null)
        {
            using CancellationTokenSource cancellation = new();
            delayUnloadCancellation = cancellation;
            await UnloadDelay.Delay(delayUnloadCancellation.Token);

            if (cancellation.IsCancellationRequested)
            {
                return;
            }
        }

        ExecuteUnload(value);
    }
}
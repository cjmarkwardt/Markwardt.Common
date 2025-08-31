namespace Markwardt;

public class AssetReservation<T> : BaseDisposable
{
    public AssetReservation(IAsset<T> asset)
        => Load(asset);

    private readonly CancellationTokenSource cancellation = new();

    private T instance = default!;

    protected override void OnDispose()
    {
        base.OnDispose();

        cancellation.CancelAndDispose();
        instance = default!;
    }

    private async void Load(IAsset<T> asset)
    {
        try
        {
            instance = await asset.Load(cancellation.Token);
        }
        catch (OperationCanceledException) { }
    }
}
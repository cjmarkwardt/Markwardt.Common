namespace Markwardt;

[SuppressMessage("", "S3881", Justification = "Custom disposable pattern")]
public abstract class BaseAsyncDisposable : ICompositeDisposable, ITrackedDisposable, IAsyncDisposable
{
    private readonly CancellationTokenSource cancellation = new();

    public bool IsDisposed { get; private set; }

    public ISet<object> DisposalTargets { get; } = new HashSet<object>();

    public CancellationToken Disposal => cancellation.Token;

    public void Dispose()
    {
        if (!IsDisposed)
        {
            PrepareDispose();
            OnDispose();
            DisposalTargets.ForEach(x => x.TryDispose());
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!IsDisposed)
        {
            PrepareDispose();
            await OnAsyncDispose();
            await Task.WhenAll(DisposalTargets.Select(x => x.TryDisposeAsync().AsTask()));
        }
    }

    protected virtual void OnPrepareDispose() { }

    protected virtual void OnSharedDispose() { }

    protected virtual void OnDispose() { }

    protected virtual ValueTask OnAsyncDispose()
        => ValueTask.CompletedTask;

    private void PrepareDispose()
    {
        IsDisposed = true;
        OnPrepareDispose();
        OnSharedDispose();

        cancellation.Cancel();
        cancellation.Dispose();
    }
}
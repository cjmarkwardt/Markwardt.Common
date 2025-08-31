namespace Markwardt;

[SuppressMessage("", "S3881", Justification = "Custom disposable pattern")]
public abstract class BaseDisposable : ICompositeDisposable, ITrackedDisposable
{
    private readonly CancellationTokenSource cancellation = new();

    public bool IsDisposed { get; private set; }

    public ISet<object> DisposalTargets { get; } = new HashSet<object>();

    public CancellationToken Disposal => cancellation.Token;

    public void Dispose()
    {
        if (!IsDisposed)
        {
            IsDisposed = true;

            OnDispose();

            cancellation.Cancel();
            cancellation.Dispose();

            DisposalTargets.ForEach(x => x.TryDispose());
        }
    }

    protected virtual void OnDispose() { }

    protected CancellationTokenSource LinkDisposal(params scoped ReadOnlySpan<CancellationToken> tokens)
        => CancellationTokenSource.CreateLinkedTokenSource([Disposal, .. tokens]);
}
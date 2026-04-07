namespace Markwardt;

[SuppressMessage("", "S3881", Justification = "Custom disposable pattern")]
public abstract class BaseDisposable : IParentDisposable, ITrackedDisposable
{
    private readonly HashSet<IDisposable> children = [];
    private readonly CancellationTokenSource cancellation = new();

    public bool IsDisposed { get; private set; }

    public CancellationToken Disposal => cancellation.Token;

    public void AddChildDisposable(object? disposable)
    {
        if (disposable is IDisposable typed)
        {
            children.Add(typed);
        }
    }

    public void RemoveChildDisposable(object? disposable)
    {
        if (disposable is IDisposable typed)
        {
            children.Remove(typed);
        }
    }

    public void Dispose()
    {
        if (!IsDisposed)
        {
            IsDisposed = true;

            OnDispose();

            cancellation.Cancel();
            cancellation.Dispose();

            children.ForEach(x => x.Dispose());
        }
    }

    protected virtual void OnDispose() { }

    protected CancellationTokenSource LinkDisposal(params scoped ReadOnlySpan<CancellationToken> tokens)
        => CancellationTokenSource.CreateLinkedTokenSource([Disposal, .. tokens]);
}
namespace Markwardt;

public sealed class SafeCancellationTokenSource : IDisposable
{
    private readonly CancellationTokenSource cancellation = new();

    public bool IsDisposed { get; private set; }
    public bool IsCancelled { get; private set; }

    public CancellationToken Token => cancellation.Token;

    public void Cancel()
    {
        IsCancelled = true;
        cancellation.Cancel();
    }

    public void TryCancel()
    {
        if (!IsDisposed && !IsCancelled)
        {
            Cancel();
        }
    }

    public void Dispose()
    {
        if (!IsDisposed)
        {
            IsDisposed = true;
            cancellation.Dispose();
        }
    }
}
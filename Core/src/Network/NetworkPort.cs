namespace Markwardt;

public interface INetworkPort : INetworkPeer
{
    ValueTask Open(CancellationToken cancellation = default);
}

public abstract class NetworkPort : BaseDisposable, INetworkPort
{
    private readonly CancellationTokenSource cancellation = new();

    private bool wasOpened;
    private BackgroundTaskk? process;
    private State state;

    public bool IsOpen => state is State.Opened;

    public Exception? Exception { get; private set; }

    public async ValueTask Open(CancellationToken cancellation = default)
    {
        Start(false);

        using CancellationTokenSource linkedCancellation = LinkCancellation(cancellation);
        await Link(linkedCancellation.Token);

        if (state is State.Opening)
        {
            state = State.Opened;
            wasOpened = true;
            OnOpened();
        }
    }

    public async ValueTask Close(CancellationToken cancellation = default)
    {
        if (state is State.Closing || state is State.Closed)
        {
            return;
        }

        try
        {
            SetClosing(null);

            using CancellationTokenSource linkedCancellation = LinkCancellation(cancellation);
            await Unlink(linkedCancellation.Token);

            if (process is not null)
            {
                process.Dispose();
                await process.Task.WaitAsync(linkedCancellation.Token);
            }
        }
        finally
        {
            InternalDrop(null);
        }
    }

    protected virtual ValueTask Run(CancellationToken cancellation)
        => ValueTask.CompletedTask;

    protected virtual ValueTask Link(CancellationToken cancellation)
        => ValueTask.CompletedTask;

    protected virtual ValueTask Unlink(CancellationToken cancellation)
        => ValueTask.CompletedTask;

    protected virtual void OnOpened() { }
    protected virtual void OnClosed(Exception? exception) { }
    protected virtual void Release() { }

    protected void Initialize()
        => Start(true);

    protected void Drop(Exception exception)
        => InternalDrop(exception);

    private void InternalDrop(Exception? exception)
    {
        if (state is State.Closed)
        {
            return;
        }

        SetClosing(exception);
        state = State.Closed;
        process?.Dispose();
        Release();
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        InternalDrop(null);
    }

    private CancellationTokenSource LinkCancellation(params ReadOnlySpan<CancellationToken> tokens)
        => LinkDisposal([cancellation.Token, .. tokens]);

    private void Start(bool isOpen)
    {
        if (state is not State.Unopened)
        {
            throw new InvalidOperationException("Already started");
        }

        if (isOpen)
        {
            state = State.Opened;
            OnOpened();
        }
        else
        {
            state = State.Opening;
        }

        process = BackgroundTaskk.Start(async cancellation => await Run(cancellation), error => { if (error is not null) { Drop(error); } });
    }

    private void SetClosing(Exception? exception)
    {
        if (state is State.Closing)
        {
            return;
        }

        state = State.Closing;
        Exception = exception;

        if (wasOpened)
        {
            OnClosed(exception);
        }
    }

    private enum State
    {
        Unopened,
        Opening,
        Opened,
        Closing,
        Closed
    }
}
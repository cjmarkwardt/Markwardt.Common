namespace Markwardt.Network;

public interface IHost<T> : IDisposable
{
    HostState State { get; }
    Exception? StopException { get; }
    IObservable<Exception?> Stopped { get; }

    IObservable<IConnection<T>> Connected { get; }
}

public static class HostExtensions
{
    public static IDisposable Handle<T>(this IHost<T> host, IHostHandler<T> handler)
    {
        CompositeDisposable disposables = [];
        host.Stopped.Subscribe(handler.OnHostStopped).DisposeWith(disposables);
        host.Connected.Subscribe(x => x.Handle(handler).DisposeWith(disposables)).DisposeWith(disposables);
        host.DisposeWith(disposables);
        return disposables;
    }
}

public abstract class Host<T> : BaseDisposable, IHost<T>, IInspectable
{
    private readonly ControlledInspectable inspectable = new();

    protected IInspectable Inspectable => inspectable.Controlled;

    public HostState State { get; private set; }
    public Exception? StopException { get; private set; }

    private readonly Subject<Exception?> stopped = new();
    public IObservable<Exception?> Stopped => stopped;

    private readonly BufferSubject<IConnection<T>> connections = new();
    public IObservable<IConnection<T>> Connected => connections;

    IDictionary<IInspectKey, object> IInspectable.Inspections => inspectable.Inspections;

    protected virtual void OnStopped() { }

    protected void ChainInspections(object target)
        => inspectable.ChainInspections(target);

    protected void Enqueue(IConnection<T> connection)
    {
        if (State is HostState.Running)
        {
            connections.OnNext(connection);
        }
        else
        {
            connection.Dispose();
        }
    }

    protected void Stop(Exception? exception = null)
    {
        if (State is not HostState.Stopped && StopException is null)
        {
            State = HostState.Stopped;
            StopException = exception;
            OnStopped();
            stopped.OnNext(exception);
        }
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        Stop();
        connections.Dispose();
        stopped.Dispose();
    }
}
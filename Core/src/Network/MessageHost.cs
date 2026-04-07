namespace Markwardt;

public interface IMessageHost<T> : IDisposable
{
    HostState State { get; }
    Exception? StopException { get; }
    IObservable<Exception?> Stopped { get; }

    IObservable<IMessageConnection<T>> Connected { get; }
}

public abstract class BaseMessageHost<T> : BaseDisposable, IMessageHost<T>, IInspectable
{
    private readonly ControlledInspectable inspectable = new();

    protected IInspectable Inspectable => inspectable.Controlled;

    public HostState State { get; private set; }
    public Exception? StopException { get; private set; }

    private readonly Subject<Exception?> stopped = new();
    public IObservable<Exception?> Stopped => stopped;

    private readonly BufferSubject<IMessageConnection<T>> connections = new();
    public IObservable<IMessageConnection<T>> Connected => connections;

    IDictionary<IInspectKey, object> IInspectable.Inspections => inspectable.Inspections;

    protected virtual void OnStopped() { }

    protected void ChainInspections(object target)
        => inspectable.ChainInspections(target);

    protected void Enqueue(IMessageConnection<T> connection)
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

public class MessageHost<T> : BaseMessageHost<T>
{
    public new void Enqueue(IMessageConnection<T> connection)
        => base.Enqueue(connection);

    public new void Stop(Exception? exception = null)
        => base.Stop(exception);
}
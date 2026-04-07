namespace Markwardt;

public abstract class MessageTarget<T> : BaseDisposable, IMessageConnection<T>, IMessageInterceptable, IInspectable
{
    private readonly ControlledInspectable inspectable = new();

    protected IInspectable Inspectable => inspectable.Controlled;

    private readonly BufferSubject<Message> received = new();
    public IObservable<Message> Received => received;

    protected virtual IEnumerable<IMessageInterceptor> Interceptors => [];

    IEnumerable<IMessageInterceptor> IMessageInterceptable.Interceptors => Interceptors;

    private ConnectionState state;
    public ConnectionState State => state;

    public Exception? DisconnectException { get; private set; }

    IDictionary<IInspectKey, object> IInspectable.Inspections => inspectable.Inspections;

    public void Send(Message message)
    {
        if (message.Content is T content)
        {
            if (State is ConnectionState.Connected)
            {
                SendContent(message, content);
            }
        }
        else
        {
            SendSignal(message);
        }
    }

    protected void ChainInspections(object target)
        => inspectable.ChainInspections(target);

    protected abstract void SendContent(Message message, T content);

    protected virtual void SendSignal(Message message) { }

    protected virtual void OnConnected()
    {
        state = ConnectionState.Connected;
        Interceptors.ForEach(x => x.Attach(this));
    }

    protected virtual void OnDisconnected(Exception? exception)
    {
        state = ConnectionState.Disconnected;
        DisconnectException = exception;
    }

    protected void TriggerReceived(Message message)
    {
        if (message.Content is ConnectedSignal signal)
        {
            OnConnected();
        }
        else if (message.Content is DisconnectedSignal disconnectedSignal)
        {
            OnDisconnected(disconnectedSignal.Exception);
        }
        
        bool isIntercepted = false;
        foreach (IMessageInterceptor interceptor in Interceptors)
        {
            if (interceptor.Intercept(this, message) is IEnumerable<Message> interception)
            {
                interception.ForEach(received.OnNext);
                isIntercepted = true;
                break;
            }
        }

        if (!isIntercepted)
        {
            received.OnNext(message);
        }
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        received.Dispose();
    }
}
namespace Markwardt.Network;

public abstract class ConnectionTarget<T> : BaseDisposable, IConnection<T>, INetworkInterceptable, IInspectable
{
    private readonly ControlledInspectable inspectable = new();

    protected IInspectable Inspectable => inspectable.Controlled;

    private readonly BufferSubject<Packet<T>> received = new();
    public IObservable<Packet<T>> Received => received;

    protected virtual IEnumerable<INetworkInterceptor> Interceptors => [];

    IEnumerable<INetworkInterceptor> INetworkInterceptable.Interceptors => Interceptors;

    private ConnectionState state;
    public ConnectionState State => state;

    public Exception? DisconnectException { get; private set; }

    IDictionary<IInspectKey, object> IInspectable.Inspections => inspectable.Inspections;

    public void Send(Packet<T> packet)
    {
        if (packet.IsContent)
        {
            if (State is ConnectionState.Connected)
            {
                SendContent(packet);
            }
        }
        else
        {
            SendSignal(packet);
        }
    }

    void INetworkInterceptable.Send(Packet packet)
        => Send(packet.As<T>());

    protected void ChainInspections(object target)
        => inspectable.ChainInspections(target);

    protected abstract void SendContent(Packet<T> packet);

    protected virtual void SendSignal(Packet<T> packet) { }

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

    protected void TriggerReceived(Packet<T> packet)
    {
        if (packet.IsSignal && packet.Signal is ConnectedSignal signal)
        {
            OnConnected();
        }
        else if (packet.IsSignal && packet.Signal is DisconnectedSignal disconnectedSignal)
        {
            OnDisconnected(disconnectedSignal.Exception);
        }
        
        bool isIntercepted = false;
        foreach (INetworkInterceptor interceptor in Interceptors)
        {
            if (interceptor.Intercept(this, packet) is IEnumerable<Packet> interception)
            {
                interception.ForEach(x => received.OnNext(x.As<T>()));
                isIntercepted = true;
                break;
            }
        }

        if (!isIntercepted)
        {
            received.OnNext(packet);
        }
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        received.Dispose();
    }
}
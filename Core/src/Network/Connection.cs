namespace Markwardt.Network;

public interface IConnection : IDisposable, ISender
{
    ConnectionState State { get; }
    Exception? DisconnectException { get; }
    IObservable<Packet> Received { get; }
}

public interface IConnection<T> : IConnection, ISender<T>
{
    new IObservable<Packet<T>> Received => ((IConnection)this).Received.Select(packet => packet.As<T>());
}

public static class ConnectionExtensions
{
    public static IDisposable Handle<T>(this IConnection<T> connection, IConnectionHandler<T> handler)
        => new CompositeDisposable()
        {
            connection.Received.Subscribe(packet =>
            {
                if (packet.IsContent)
                {
                    handler.OnReceived(connection, packet);
                }
                else if (packet.Signal is ConnectedSignal)
                {
                    handler.OnConnected(connection);
                }
                else if (packet.Signal is DisconnectedSignal disconnectSignal)
                {
                    handler.OnDisconnected(connection, disconnectSignal.Exception);
                }
                else
                {
                    handler.OnSignalReceived(connection, packet);
                }
            }),
            connection
        };
}

public abstract class Connection<T> : ConnectionTarget<T>
{
    protected void SetConnected()
    {
        if (State is ConnectionState.Connecting)
        {
            TriggerReceived(Packet.NewSignal<object?>(ConnectedSignal.Instance));
        }
    }

    protected void SetDisconnected(Exception? exception = null)
    {
        if (State is not ConnectionState.Disconnected)
        {
            TriggerReceived(Packet.NewSignal<object?>(new DisconnectedSignal(exception)));
        }
    }

    protected override void OnDispose()
    {
        SetDisconnected();

        base.OnDispose();
    }
}
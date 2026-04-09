namespace Markwardt.Network;

public interface IConnection : IDisposable, ISender
{
    ConnectionState State { get; }
    Exception? DisconnectException { get; }
    IObservable<Packet> Received { get; }
}

public interface IConnection<T> : IConnection, ISender<T>
{
    IObservable<Packet<T>> ContentReceived => Received.Select(packet => packet.AsContent<T>());
}

public static class ConnectionExtensions
{
    public static IDisposable Handle<T>(this IConnection<T> connection, IConnectionHandler<T> handler)
        => new CompositeDisposable()
        {
            connection.Received.Subscribe(packet =>
            {
                if (packet.Content is ConnectedSignal)
                {
                    handler.OnConnected(connection);
                }
                else if (packet.Content is DisconnectedSignal disconnectSignal)
                {
                    handler.OnDisconnected(connection, disconnectSignal.Exception);
                }
                else if (packet.Content is T content)
                {
                    handler.OnReceived(connection, packet.AsContent<T>());
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
            TriggerReceived(Packet.New(ConnectedSignal.Instance));
        }
    }

    protected void SetDisconnected(Exception? exception = null)
    {
        if (State is not ConnectionState.Disconnected)
        {
            TriggerReceived(Packet.New(new DisconnectedSignal(exception)));
        }
    }

    protected override void OnDispose()
    {
        SetDisconnected();

        base.OnDispose();
    }
}
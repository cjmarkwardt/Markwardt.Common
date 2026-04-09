namespace Markwardt.Network;

public interface IConnectionHandler<T>
{
    void OnConnected(IConnection<T> connection);
    void OnReceived(IConnection<T> connection, Packet<T> packet);
    void OnSignalReceived(IConnection<T> connection, Packet packet);
    void OnDisconnected(IConnection<T> connection, Exception? exception);
}

public record ConnectionHandler<T> : IConnectionHandler<T>
{
    public Action<IConnection<T>>? ConnectedHandler { get; init; }
    public Action<IConnection<T>, Packet<T>>? ReceivedHandler { get; init; }
    public Action<IConnection<T>, Packet>? SignalReceivedHandler { get; init; }
    public Action<IConnection<T>, Exception?>? DisconnectedHandler { get; init; }

    public void OnConnected(IConnection<T> connection)
        => ConnectedHandler?.Invoke(connection);

    public void OnReceived(IConnection<T> connection, Packet<T> packet)
        => ReceivedHandler?.Invoke(connection, packet);

    public void OnSignalReceived(IConnection<T> connection, Packet packet)
        => SignalReceivedHandler?.Invoke(connection, packet);

    public void OnDisconnected(IConnection<T> connection, Exception? exception)
        => DisconnectedHandler?.Invoke(connection, exception);
}
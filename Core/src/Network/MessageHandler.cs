namespace Markwardt;

public interface IMessageHandler<T>
{
    void OnConnected(IMessageConnection<T> connection);
    void OnReceived(IMessageConnection<T> connection, Message message, T content);
    void OnSignalReceived(IMessageConnection<T> connection, Message message, object? signal);
    void OnDisconnected(IMessageConnection<T> connection, Exception? exception);
    void OnStopped(Exception? exception);
}

public record MessageHandler<T> : IMessageHandler<T>
{
    public Action<IMessageConnection<T>>? ConnectedHandler { get; init; }
    public Action<IMessageConnection<T>, Message, T>? ReceivedHandler { get; init; }
    public Action<IMessageConnection<T>, Message, object?>? SignalReceivedHandler { get; init; }
    public Action<IMessageConnection<T>, Exception?>? DisconnectedHandler { get; init; }
    public Action<Exception?>? StoppedHandler { get; init; }

    public void OnConnected(IMessageConnection<T> connection)
        => ConnectedHandler?.Invoke(connection);

    public void OnReceived(IMessageConnection<T> connection, Message message, T content)
        => ReceivedHandler?.Invoke(connection, message, content);

    public void OnSignalReceived(IMessageConnection<T> connection, Message message, object? signal)
        => SignalReceivedHandler?.Invoke(connection, message, signal);

    public void OnDisconnected(IMessageConnection<T> connection, Exception? exception)
        => DisconnectedHandler?.Invoke(connection, exception);

    public void OnStopped(Exception? exception)
        => StoppedHandler?.Invoke(exception);
}
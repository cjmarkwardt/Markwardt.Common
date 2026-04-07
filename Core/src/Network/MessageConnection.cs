namespace Markwardt;

public interface IMessageConnection : IDisposable, IMessageSender
{
    ConnectionState State { get; }
    Exception? DisconnectException { get; }
    IObservable<Message> Received { get; }
}

public interface IMessageConnection<T> : IMessageConnection, IMessageSender<T>;

public static class MessageConnectionExtensions
{
    public static IDisposable Handle<T>(this IMessageConnection<T> connection, IMessageHandler<T> handler)
        => new CompositeDisposable()
        {
            connection.Received.Subscribe(message =>
            {
                if (message.Content is ConnectedSignal)
                {
                    handler.OnConnected(connection);
                }
                else if (message.Content is DisconnectedSignal disconnectSignal)
                {
                    handler.OnDisconnected(connection, disconnectSignal.Exception);
                }
                else if (message.Content is T content)
                {
                    handler.OnReceived(connection, message, content);
                }
                else
                {
                    handler.OnSignalReceived(connection, message, message.Content);
                }
            }),
            connection
        };

    public static IDisposable Handle<T>(this IMessageHost<T> host, IMessageHandler<T> handler)
    {
        CompositeDisposable disposables = [];
        host.Stopped.Subscribe(handler.OnStopped).DisposeWith(disposables);
        host.Connected.Subscribe(x => x.Handle(handler).DisposeWith(disposables)).DisposeWith(disposables);
        host.DisposeWith(disposables);
        return disposables;
    }
}

public abstract class MessageConnection<T> : MessageTarget<T>
{
    protected void SetConnected()
    {
        if (State is ConnectionState.Connecting)
        {
            TriggerReceived(Message.New(ConnectedSignal.Instance));
        }
    }

    protected void SetDisconnected(Exception? exception = null)
    {
        if (State is not ConnectionState.Disconnected)
        {
            TriggerReceived(Message.New(new DisconnectedSignal(exception)));
        }
    }

    protected override void OnDispose()
    {
        SetDisconnected();

        base.OnDispose();
    }
}
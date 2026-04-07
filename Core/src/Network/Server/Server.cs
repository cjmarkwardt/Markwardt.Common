namespace Markwardt;

public record WorldMessage
{
    
}

public interface IWorldClient
{
    IObservable<Unit> Connected { get; }
    IObservable<Exception?> Disconnected { get; }
    IObservable<WorldMessage> Received { get; }

    void Connect(IMessageConnection<WorldMessage> connection);
}

public class WorldClient : BaseDisposable, IWorldClient, IMessageHandler<WorldMessage>
{
    private IMessageConnection<WorldMessage>? connection;

    private readonly BufferSubject<Unit> connected = new();
    public IObservable<Unit> Connected => connected;

    private readonly BufferSubject<Exception?> disconnected = new();
    public IObservable<Exception?> Disconnected => disconnected;

    private readonly BufferSubject<WorldMessage> received = new();
    public IObservable<WorldMessage> Received => received;

    public void Connect(IMessageConnection<WorldMessage> connection)
    {
        if (this.connection is not null)
        {
            throw new InvalidOperationException("Already connected");
        }

        this.connection = connection;
        connection.Handle(this).DisposeWith(this);
    }

    void IMessageHandler<WorldMessage>.OnConnected(IMessageConnection<WorldMessage> connection)
        => connected.OnNext(Unit.Default);

    void IMessageHandler<WorldMessage>.OnDisconnected(IMessageConnection<WorldMessage> connection, Exception? exception)
        => disconnected.OnNext(exception);

    void IMessageHandler<WorldMessage>.OnReceived(IMessageConnection<WorldMessage> connection, Message message, WorldMessage content)
        => received.OnNext(content);

    void IMessageHandler<WorldMessage>.OnSignalReceived(IMessageConnection<WorldMessage> connection, Message message, object? signal) { }

    void IMessageHandler<WorldMessage>.OnStopped(Exception? exception) { }
}

public interface IWorldServer
{
    
}
namespace Markwardt;

public interface INetworkMessageServer : INetworkPort, INetworkMessageHost
{
    INetworkMessageServerHandler? Handler { get; set; }
}

public class NetworkMessageServer : BaseDisposable, INetworkMessageServer, INetworkServerHandler
{
    public NetworkMessageServer(INetworkServer server, INetworkMessageSerializer serializer)
    {
        this.server = server;
        this.serializer = serializer;
        
        server.Handler = this;
    }

    private readonly INetworkServer server;
    private readonly INetworkMessageSerializer serializer;
    private readonly Dictionary<INetworkConnection, Connection> connections = [];

    public INetworkMessageServerHandler? Handler { get; set; }

    public IEnumerable<INetworkMessageConnection> Connections => connections.Values;
    public bool IsOpen => server.IsOpen;
    public Exception? Exception => server.Exception;

    public async ValueTask Open(CancellationToken cancellation = default)
        => await server.Open(cancellation);

    public async ValueTask Close(CancellationToken cancellation = default)
    {
        await Task.WhenAll(connections.Values.Select(x => x.Close(cancellation).AsTask()));
        await server.Close(cancellation);
    }

    void INetworkServerHandler.OnConnected(INetworkConnection connection)
    {
        Connection messageConnection = new(this, connection, serializer);
        connections.Add(connection, messageConnection);
        Handler?.OnConnected(messageConnection);
    }

    void INetworkServerHandler.OnReceived(INetworkConnection connection, ReadOnlySpan<byte> data)
    {
        if (connections.TryGetValue(connection, out Connection? messageConnection))
        {
            messageConnection.Receive(data);
        }
    }

    void INetworkServerHandler.OnDisconnected(INetworkConnection connection, Exception? exception)
    {
        if (connections.TryGetValue(connection, out Connection? messageConnection))
        {
            connections.Remove(connection);
            Handler?.OnDisconnected(messageConnection, exception);
        }
    }

    void INetworkServerHandler.OnOpened()
        => Handler?.OnOpened();

    void INetworkServerHandler.OnClosed(Exception? exception)
        => Handler?.OnClosed(exception);

    protected override void OnDispose()
    {
        base.OnDispose();
        connections.Values.ToList().ForEach(x => x.Dispose());
        server.Dispose();
    }

    private class Connection : BaseDisposable, INetworkMessageConnection, INetworkMessageProcessorHandler
    {
        public Connection(NetworkMessageServer server, INetworkConnection connection, INetworkMessageSerializer serializer)
        {
            this.server = server;
            this.connection = connection;
            processor = new(connection, this, serializer) { Handler = this };
        }

        private readonly NetworkMessageServer server;
        private readonly INetworkConnection connection;
        private readonly NetworkMessageProcessor processor;

        public IReadOnlyDictionary<int, INetworkChannel> Channels => processor.Channels;
        public bool IsOpen => connection.IsOpen;
        public Exception? Exception => connection.Exception;

        public INetworkChannel CreateChannel(int id)
            => processor.CreateChannel(id);

        public void Send(object message, NetworkConstraints constraints = NetworkConstraints.All)
            => processor.Send(message, constraints);

        public async ValueTask<object> Request(object request, TimeSpan? timeout = null, CancellationToken cancellation = default)
            => await processor.Request(request, timeout, cancellation);

        public void Receive(ReadOnlySpan<byte> data)
            => processor.Receive(data);

        public async ValueTask Close(CancellationToken cancellation = default)
            => await connection.Close(cancellation);

        void INetworkMessageProcessorHandler.OnReceived(object message, INetworkChannel? channel)
            => server.Handler?.OnReceived(this, message, channel);

        async ValueTask<object> INetworkMessageProcessorHandler.OnRequested(object request)
        {
            if (server.Handler is null)
            {
                throw new NetworkRequestRejectedException("Requests are not handled");
            }

            return await server.Handler.OnRequested(this, request);
        }

        void INetworkMessageProcessorHandler.OnRecycled(object message)
            => server.Handler?.OnRecycled(message);

        protected override void OnDispose()
        {
            base.OnDispose();
            connection.Dispose();
        }
    }
}

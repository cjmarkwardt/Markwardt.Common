namespace Markwardt;

public interface INetworkServer : INetworkPort, INetworkHost
{
    INetworkServerHandler? Handler { get; set; }
}

public abstract class NetworkServer<TConnection> : NetworkPort, INetworkServer
{
    private readonly ExtendedDictionary<TConnection, Client> clients = [];

    public INetworkServerHandler? Handler { get; set; }

    public IEnumerable<INetworkConnection> Connections => clients.Values;

    protected abstract ValueTask ExecuteSend(TConnection connection, ReadOnlyMemory<byte> data, NetworkConstraints constraints, CancellationToken cancellation);

    protected virtual ValueTask RunConnection(TConnection connection, CancellationToken cancellation)
        => ValueTask.CompletedTask;

    protected virtual void ReleaseConnection(TConnection connection) { }

    protected virtual ValueTask UnlinkConnection(TConnection connection, CancellationToken cancellation)
        => ValueTask.CompletedTask;

    protected override void OnOpened()
        => Handler?.OnOpened();

    protected override void OnClosed(Exception? exception)
        => Handler?.OnClosed(exception);

    protected override async ValueTask Unlink(CancellationToken cancellation)
        => await Task.WhenAll(clients.Values.Select(x => x.Close(cancellation).AsTask()));

    protected override void Release()
        => clients.Values.ToList().ForEach(x => x.Dispose());

    protected INetworkConnection Connect(Func<INetworkConnectionController, TConnection> createPeer)
    {
        Client client = new(this);
        TConnection connection = createPeer(client);
        client.Initialize(connection);
        return client;
    }

    protected void Receive(TConnection connection, ReadOnlySpan<byte> data)
        => clients.GetValueOrDefault(connection)?.Receive(data);

    protected void Disconnect(TConnection connection, Exception exception)
        => clients.GetValueOrDefault(connection)?.Drop(exception);

    private class Client(NetworkServer<TConnection> server) : BaseNetworkClient
    {
        private Maybe<TConnection> connection;

        public void Initialize(TConnection connection)
        {
            this.connection = connection.Maybe();
            Initialize();
        }

        public void Receive(ReadOnlySpan<byte> data)
            => ExecuteReceive(data);

        public new void Drop(Exception exception)
            => base.Drop(exception);

        protected override async ValueTask Run(CancellationToken cancellation)
            => await server.RunConnection(connection.Value, cancellation);

        protected override ValueTask Link(CancellationToken cancellation)
            => ValueTask.CompletedTask;

        protected override async ValueTask ExecuteSend(ReadOnlyMemory<byte> data, NetworkConstraints constraints, CancellationToken cancellation)
            => await server.ExecuteSend(connection.Value, data, constraints, cancellation);

        protected override void ExecuteReceive(ReadOnlySpan<byte> data)
            => server.Handler?.OnReceived(this, data);

        protected override async ValueTask Unlink(CancellationToken cancellation)
            => await server.UnlinkConnection(connection.Value, cancellation);

        protected override void OnOpened()
        {
            server.clients.Add(connection.Value, this);
            server.Handler?.OnConnected(this);
        }

        protected override void OnClosed(Exception? exception)
        {
            server.clients.Remove(connection.Value);
            server.Handler?.OnDisconnected(this, exception);
        }

        protected override void Release()
            => server.ReleaseConnection(connection.Value);
    }
}
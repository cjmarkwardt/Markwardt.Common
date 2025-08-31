namespace Markwardt;

public interface INetworkMessageServerGroup : IDisposable
{
    IEnumerable<INetworkMessageHost> Servers { get; }
    IEnumerable<INetworkMessageConnection> Connections { get; }

    INetworkMessageServerGroupHandler? Handler { get; set; }

    ValueTask<INetworkMessageHost> Open(INetworkServer server, object? tag = null, CancellationToken cancellation = default);
}

public class NetworkMessageServerGroup(INetworkMessageSerializer serializer) : BaseDisposable, INetworkMessageServerGroup
{
    private readonly INetworkMessageSerializer serializer = serializer;

    private readonly HashSet<Host> servers = [];
    public IEnumerable<INetworkMessageHost> Servers => servers;

    public IEnumerable<INetworkMessageConnection> Connections => servers.SelectMany(x => x.Connections);

    public INetworkMessageServerGroupHandler? Handler { get; set; }

    public async ValueTask<INetworkMessageHost> Open(INetworkServer server, object? tag = null, CancellationToken cancellation = default)
    {
        Host? host = null;
        try
        {
            host = new(this, server, tag);
            await host.Open(cancellation);
            return host;
        }
        catch
        {
            host?.Dispose();
            throw;
        }
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        foreach (Host host in servers)
        {
            host.Dispose();
        }

        servers.Clear();
    }

    private class Host : NetworkMessageServer, INetworkMessageServerHandler
    {
        public Host(NetworkMessageServerGroup group, INetworkServer server, object? tag)
            : base(server, group.serializer)
        {
            this.group = group;
            this.server = new NetworkMessageServer(server, group.serializer).DisposeWith(this);
            this.tag = tag;

            this.server.Handler = this;
        }

        private readonly NetworkMessageServerGroup group;
        private readonly NetworkMessageServer server;
        private readonly object? tag;

        void INetworkMessageServerHandler.OnConnected(INetworkMessageConnection connection)
            => group.Handler?.OnConnected(this, connection);

        void INetworkMessageServerHandler.OnReceived(INetworkMessageConnection connection, object message, INetworkChannel? channel)
            => group.Handler?.OnReceived(this, connection, message, channel);

        async ValueTask<object> INetworkMessageServerHandler.OnRequested(INetworkMessageConnection connection, object request)
        {
            if (group.Handler is null)
            {
                throw new NetworkRequestRejectedException("Requests are not handled");
            }

            return await group.Handler.OnRequested(this, connection, request);
        }

        void INetworkMessageServerHandler.OnRecycled(object message)
            => group.Handler?.OnRecycled(message);

        void INetworkMessageServerHandler.OnDisconnected(INetworkMessageConnection connection, Exception? exception)
            => group.Handler?.OnDisconnected(this, connection, exception);

        void INetworkMessageServerHandler.OnOpened()
        {
            group.servers.Add(this);
            group.Handler?.OnOpened(this, tag);
        }

        void INetworkMessageServerHandler.OnClosed(Exception? exception)
        {
            group.servers.Remove(this);
            group.Handler?.OnClosed(this, exception);
        }
    }
}
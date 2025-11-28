namespace Markwardt;

public interface IWorldManager<TWorld, TWorldConfiguration> : IDisposable
{
    WorldManagerState State { get; }
    IWorldServer<TWorld>? Server { get; }
    IWorldPlayer? Player { get; }
    INetworkSerializerConfiguration MessageConfiguration { get; }

    IAsyncEnumerable<string> List();
    ValueTask Create(string world, TWorldConfiguration configuration, CancellationToken cancellation = default);
    ValueTask Load(string world, CancellationToken cancellation = default);
    ValueTask Join(INetworkConnector connector, string name, string? password = null, CancellationToken cancellation = default);
    ValueTask Delete(string world, CancellationToken cancellation = default);
    void Shutdown();
}

public static class WorldManagerExtensions
{
    public static void Reset<TWorld, TWorldConfiguration>(this IWorldManager<TWorld, TWorldConfiguration> manager)
    {
        manager.Leave();
        manager.Shutdown();
    }

    public static void Leave<TWorld, TWorldConfiguration>(this IWorldManager<TWorld, TWorldConfiguration> manager)
        => manager.Player?.Dispose();

    public static async ValueTask Leave<TWorld, TWorldConfiguration>(this IWorldManager<TWorld, TWorldConfiguration> manager, string reason, CancellationToken cancellation = default)
    {
        if (manager.Player is not null)
        {
            await manager.Player.Connection.Disconnect(reason, cancellation);
        }
    }
}

public abstract class WorldManager<TWorld, TWorldConfiguration> : BaseDisposable, IWorldManager<TWorld, TWorldConfiguration>, INetworkHandler
{
    public WorldManager(IWorldGenerator<TWorld, TWorldConfiguration> generator, IWorldStore<TWorld> store, IWorldServerHandler<TWorld> serverHandler, IWorldClientHandler clientHandler, IConfiguredNetworkSerializer? serializer = null, INetworkAuthenticator? authenticator = null)
    {
        this.generator = generator;
        this.store = store;
        this.serverHandler = serverHandler;
        this.clientHandler = clientHandler;
        this.serializer = serializer ?? new JsonNetworkSerializer();
        network = new(this, this.serializer, authenticator: authenticator);

        this.serializer.ConfigureType(typeof(WorldJoinRequest));
        this.serializer.ConfigureType(typeof(WorldJoinResponse));
    }

    private readonly IWorldGenerator<TWorld, TWorldConfiguration> generator;
    private readonly IWorldStore<TWorld> store;
    private readonly IWorldServerHandler<TWorld> serverHandler;
    private readonly IWorldClientHandler clientHandler;
    private readonly IConfiguredNetworkSerializer serializer;
    private readonly NetworkManager network;
    private readonly ReadyTracker readyTracker = new();

    public WorldManagerState State { get; private set; }
    public IWorldServer<TWorld>? Server { get; private set; }
    public IWorldPlayer? Player { get; private set; }

    public INetworkSerializerConfiguration MessageConfiguration => serializer;

    public IAsyncEnumerable<string> List()
        => store.List();

    public async ValueTask Create(string world, TWorldConfiguration configuration, CancellationToken cancellation = default)
    {
        using IDisposable ready = readyTracker.Start();
        this.Reset();
        StartServer(world, await generator.Generate(configuration, cancellation));
    }

    public async ValueTask Load(string world, CancellationToken cancellation = default)
    {
        using IDisposable ready = readyTracker.Start();
        this.Reset();
        State = WorldManagerState.Loading;
        StartServer(world, await store.Load(world, cancellation));
    }

    public async ValueTask Join(INetworkConnector connector, string name, string? password = null, CancellationToken cancellation = default)
    {
        using IDisposable ready = readyTracker.Start();
        this.Reset();
        State = WorldManagerState.Joining;
        await network.Connect(connector, new WorldJoinRequest(name, password), new WorldPlayer(name, true), cancellation: cancellation);
    }

    public async ValueTask Delete(string world, CancellationToken cancellation = default)
    {
        using IDisposable ready = readyTracker.Start();

        if (world == Server?.WorldName)
        {
            await Server.Delete(cancellation);
        }
        else
        {
            await store.Delete(world, cancellation);
        }
    }

    public void Shutdown()
    {
        if (Server is not null)
        {
            State = WorldManagerState.Offline;
            serverHandler?.OnStopped();

            Server.Hosts.ForEach(x => x.Dispose());
            Server.Players.ForEach(x => x.Dispose());
            Server = null;
        }
    }

    protected abstract void Recycle(object message);

    private void StartServer(string name, TWorld world)
    {
        Server = new WorldServer(this, name, world);
        State = WorldManagerState.Server;
        serverHandler.OnStarted(Server);
    }

    ValueTask<object?> INetworkHandler.OnRegistration(INetworkHost? host, string identifier, ReadOnlyMemory<byte> verifier, object? details, bool isLocal, CancellationToken cancellation)
        => throw NetworkException.Unhandled;

    ValueTask<(object? UserProfile, ReadOnlyMemory<byte> Verifier)> INetworkHandler.OnAuthentication(INetworkHost? host, string identifier, bool isLocal, CancellationToken cancellation)
        => throw NetworkException.Unhandled;

    ValueTask<(object? Response, object? ConnectionProfile)> INetworkHandler.OnConnection(INetworkHost? host, NetworkUser? user, object? request, bool isLocal, bool isSecure, CancellationToken cancellation)
    {
        if (Server is not null && request is WorldJoinRequest joinRequest)
        {
            if (!isLocal && Server.Password is not null && joinRequest.Password != Server.Password)
            {
                throw new NetworkException("Invalid password");
            }
            else if (Server.Players.Any(x => x.Name == joinRequest.Name))
            {
                throw new NetworkException("Name already in use");
            }

            bool isAdmin = isLocal || joinRequest.Password == Server.AdminPassword;
            WorldPlayer player = new(joinRequest.Name, false) { IsAdmin = isAdmin };
            return ValueTask.FromResult<(object? Response, object? ConnectionProfile)>((new WorldJoinResponse(isAdmin), player));
        }
        else
        {
            throw new NetworkException("Invalid join request");
        }
    }

    void INetworkHandler.OnConnected(INetworkConnection connection, object? message)
    {
        if (connection.Profile is WorldPlayer player)
        {
            player.Connection = connection;

            if (message is WorldJoinResponse response)
            {
                player.IsAdmin = response.IsAdmin;
            }
                
            if (player.IsCurrent)
            {
                Player = player;
                clientHandler.OnConnected(player);
            }
            else
            {
                serverHandler.OnConnected(player);
            }
        }
    }

    void INetworkHandler.OnReceived(INetworkConnection connection, object? channel, object message)
    {
        if (connection.Profile is IWorldPlayer player)
        {
            if (player.IsCurrent)
            {
                clientHandler?.OnReceived(channel, message);
            }
            else
            {
                serverHandler?.OnReceived(player, channel, message);
            }
        }
    }

    async ValueTask<(object Response, NetworkSecurity? Security, INetworkBlockPool? Pool)> INetworkHandler.OnRequested(INetworkConnection connection, object message, CancellationToken cancellation)
    {
        if (connection.Profile is IWorldPlayer player)
        {
            if (player.IsCurrent)
            {
                if (clientHandler is not null)
                {
                    return await clientHandler.OnRequested(message, cancellation);
                }
            }
            else
            {
                if (serverHandler is not null)
                {
                    return await serverHandler.OnRequested(player, message, cancellation);
                }
            }
        }

        throw NetworkException.Unhandled;
    }

    (object ChannelProfile, NetworkSecurity? Security, INetworkBlockPool? Pool) INetworkHandler.OnChannelOpened(INetworkConnection connection, object message)
    {
        if (connection.Profile is IWorldPlayer player)
        {
            if (player.IsCurrent)
            {
                if (clientHandler is not null)
                {
                    return clientHandler.OnChannelOpened(message);
                }
            }
            else
            {
                if (serverHandler is not null)
                {
                    return serverHandler.OnChannelOpened(player, message);
                }
            }
        }

        return (new AnonymousChannel(), null, null);
    }

    void INetworkHandler.OnChannelClosed(INetworkConnection connection, object? profile)
    {
        if (connection.Profile is IWorldPlayer player)
        {
            if (player.IsCurrent)
            {
                clientHandler?.OnChannelClosed(profile);
            }
            else
            {
                serverHandler?.OnChannelClosed(player, profile);
            }
        }

        throw NetworkException.Unhandled;
    }

    void INetworkHandler.OnDisconnected(INetworkConnection connection, Exception? exception)
    {
        if (connection.Profile is IWorldPlayer player)
        {
            if (player.IsCurrent)
            {
                clientHandler?.OnDisconnected(exception);
                Player = null;

                if (State is WorldManagerState.Joined)
                {
                    State = WorldManagerState.Offline;
                }
            }
            else
            {
                serverHandler?.OnDisconnected(player, exception);
            }
        }
    }

    void INetworkHandler.OnRecycled(object message)
    {
        
    }

    private sealed class ReadyTracker
    {
        public ReadyTracker()
            => disposer = new(this);

        private readonly Disposer disposer;

        public bool IsReady { get; private set; } = true;

        public IDisposable Start()
        {
            if (!IsReady)
            {
                throw new InvalidOperationException("Not ready");
            }

            IsReady = false;
            return disposer;
        }

        private sealed class Disposer(ReadyTracker tracker) : IDisposable
        {
            public void Dispose()
                => tracker.IsReady = true;
        }
    }

    private sealed class WorldServer(WorldManager<TWorld, TWorldConfiguration> manager, string name, TWorld world) : IWorldServer<TWorld>
    {
        public string WorldName { get; private set; } = name;

        public TWorld World => world;
        public IEnumerable<INetworkHost> Hosts => manager.network.Hosts;
        public IEnumerable<IWorldPlayer> Players => manager.network.Connections.Select(x => x.Profile).OfType<IWorldPlayer>();

        public string? Password { get; set; }
        public string? AdminPassword { get; set; }

        public IWorldPlayer? FindPlayer(int id)
            => manager.network.FindConnection(id)?.Profile as IWorldPlayer;

        public void Rename(string name)
            => WorldName = name;

        public void Host(INetworkHoster hoster, object? profile = null)
            => manager.network.Host(hoster, profile);

        public async ValueTask Join(string name, CancellationToken cancellation = default)
        {
            if (manager.Player is not null)
            {
                throw new InvalidOperationException("Already joined");
            }

            await manager.network.Connect(new WorldJoinRequest(name, null), new WorldPlayer(name, true), cancellation);
        }

        public async ValueTask Broadcast(IEnumerable<IWorldPlayer> players, object message, NetworkSecurity? security = null, NetworkReliability mode = NetworkReliability.Reliable, CancellationToken cancellation = default)
            => await manager.network.Broadcast(players.Select(x => x.Connection), message, security, mode, cancellation: cancellation);

        public async ValueTask Save(CancellationToken cancellation = default)
        {
            using IDisposable ready = manager.readyTracker.Start();
            await manager.store.Save(WorldName, World, cancellation);
        }

        public async ValueTask Delete(CancellationToken cancellation = default)
        {
            using IDisposable ready = manager.readyTracker.Start();
            manager.Shutdown();
            await manager.store.Delete(WorldName, cancellation);
        }
    }
}
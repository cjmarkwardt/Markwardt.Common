namespace Markwardt;

public interface INetworkManager : IDisposable
{
    IEnumerable<INetworkHost> Hosts { get; }
    IEnumerable<INetworkConnection> Connections { get; }
    IEnumerable<INetworkGroupChannel> Channels { get; }

    INetworkBlockPool? DefaultPool { get; set; }
    TimeSpan HostIdReuseDelay { get; set; }
    TimeSpan ConnectionIdReuseDelay { get; set; }
    TimeSpan GroupChannelIdReuseDelay { get; set; }

    INetworkHost Host(INetworkHoster hoster, object? profile = null);
    ValueTask<INetworkConnection> Connect(INetworkConnector connector, object? request = null, object? profile = null, bool? isSecure = null, CancellationToken cancellation = default);
    ValueTask<INetworkConnection> Connect(object? request = null, object? profile = null, CancellationToken cancellation = default);
    ValueTask<INetworkConnection> Register(INetworkConnector connector, string identifier, string secret, object? details = null, object? request = null, object? profile = null, CancellationToken cancellation = default);
    ValueTask<INetworkConnection> Register(string identifier, string secret, object? details = null, object? request = null, object? profile = null, CancellationToken cancellation = default);
    ValueTask<INetworkConnection> Authenticate(INetworkConnector connector, string identifier, string secret, object? request = null, object? profile = null, CancellationToken cancellation = default);
    ValueTask<INetworkConnection> Authenticate(string identifier, string secret, object? request = null, object? profile = null, CancellationToken cancellation = default);
    INetworkConnection? FindConnection(int id);
    INetworkGroupChannel CreateGroupChannel(object? defaultMessage = null, object? profile = null, NetworkSecurity? security = null, INetworkBlockPool? pool = null);
    ValueTask Broadcast(IEnumerable<INetworkConnection> connections, object message, NetworkSecurity? security = null, NetworkReliability reliability = NetworkReliability.Reliable, INetworkBlockPool? pool = null, CancellationToken cancellation = default);
}

public class NetworkManager : BaseDisposable, INetworkManager, INetworkHostProvider
{
    public NetworkManager(INetworkHandler handler, INetworkSerializer serializer, bool secureByDefault = false, INetworkAuthenticator? authenticator = null)
    {
        this.handler = handler;
        this.secureByDefault = secureByDefault;
        this.authenticator = authenticator ?? new SecureRemotePasswordAuthenticator();
        sender = new(serializer, secureByDefault);
        receiver = new(serializer);
        hostTracker = new(0) { ReuseDelay = TimeSpan.FromMinutes(10) };
        connectionTracker = new(0) { ReuseDelay = TimeSpan.FromMinutes(10) };
        channelTracker = new(1, true) { ReuseDelay = TimeSpan.FromSeconds(30) };

        BackgroundTaskk.Start(SyncChannels).DisposeWith(this);
    }

    private readonly INetworkHandler handler;
    private readonly bool secureByDefault;
    private readonly INetworkAuthenticator authenticator;
    private readonly NetworkFormatSender sender;
    private readonly NetworkFormatReceiver receiver;
    private readonly NetworkTracker<INetworkHost> hostTracker;
    private readonly NetworkTracker<INetworkConnection> connectionTracker;
    private readonly NetworkTracker<INetworkRemoteChannel> channelTracker;
    private readonly HashSet<INetworkRemoteChannel> unsyncedChannels = [];

    public IEnumerable<INetworkHost> Hosts => hostTracker.List();
    public IEnumerable<INetworkConnection> Connections => connectionTracker.List();
    public IEnumerable<INetworkGroupChannel> Channels => channelTracker.List();

    public INetworkBlockPool? DefaultPool { get => sender.DefaultPool; set => sender.DefaultPool = value; }
    public TimeSpan HostIdReuseDelay { get => hostTracker.ReuseDelay; set => hostTracker.ReuseDelay = value; }
    public TimeSpan ConnectionIdReuseDelay { get => connectionTracker.ReuseDelay; set => connectionTracker.ReuseDelay = value; }
    public TimeSpan GroupChannelIdReuseDelay { get => channelTracker.ReuseDelay; set => channelTracker.ReuseDelay = value; }

    INetworkTracker<INetworkHost> INetworkHostProvider.HostTracker => hostTracker;
    INetworkTracker<INetworkConnection> INetworkConnectionProvider.ConnectionTracker => connectionTracker;
    INetworkTracker<INetworkRemoteChannel> INetworkChannelProvider.ChannelTracker => channelTracker;
    INetworkAuthenticator INetworkConnectionProvider.Authenticator => authenticator;
    INetworkHandler INetworkChannelProvider.Handler => handler;
    INetworkFormatSender INetworkChannelProvider.Sender => sender;
    INetworkFormatReceiver INetworkConnectionProvider.Receiver => receiver;

    public INetworkConnection? FindConnection(int id)
        => connectionTracker.Find(id);

    public INetworkGroupChannel CreateGroupChannel(object? defaultMessage = null, object? profile = null, NetworkSecurity? security = null, INetworkBlockPool? pool = null)
        => new NetworkChannel(channelTracker, this, defaultMessage, profile, security, pool);

    public INetworkHost Host(INetworkHoster hoster, object? profile = null)
        => new NetworkRemoteHost(this, hoster, profile);

    public async ValueTask<INetworkConnection> Connect(INetworkConnector connector, object? request = null, object? profile = null, bool? isSecure = null, CancellationToken cancellation = default)
        => await NetworkRemoteConnection.Connect(this, connector, request, profile, isSecure ?? secureByDefault, cancellation);

    public async ValueTask<INetworkConnection> Connect(object? request = null, object? profile = null, CancellationToken cancellation = default)
        => await NetworkLocalConnection.Connect(this, request, profile, null, cancellation);

    public async ValueTask<INetworkConnection> Register(INetworkConnector connector, string identifier, string secret, object? details = null, object? request = null, object? profile = null, CancellationToken cancellation = default)
        => await NetworkRemoteConnection.Register(this, connector, identifier, secret, details, request, profile, cancellation);

    public async ValueTask<INetworkConnection> Register(string identifier, string secret, object? details = null, object? request = null, object? profile = null, CancellationToken cancellation = default)
    {
        byte[] verifier = authenticator.CreateVerifier(identifier, secret);
        object? userProfile = await handler.OnRegistration(null, identifier, verifier, details, true, cancellation);
        authenticator.Verify(identifier, secret, verifier);
        return await NetworkLocalConnection.Connect(this, request, profile, new(identifier, userProfile), cancellation);
    }

    public async ValueTask<INetworkConnection> Authenticate(INetworkConnector connector, string identifier, string secret, object? request = null, object? profile = null, CancellationToken cancellation = default)
        => await NetworkRemoteConnection.Authenticate(this, connector, identifier, secret, request, profile, cancellation);

    public async ValueTask<INetworkConnection> Authenticate(string identifier, string secret, object? request = null, object? profile = null, CancellationToken cancellation = default)
    {
        (object? userProfile, ReadOnlyMemory<byte> verifier) = await handler.OnAuthentication(null, identifier, true, cancellation);
        authenticator.Verify(identifier, secret, verifier.Span);
        return await NetworkLocalConnection.Connect(this, request, profile, new(identifier, userProfile), cancellation);
    }

    public async ValueTask Broadcast(IEnumerable<INetworkConnection> connections, object message, NetworkSecurity? security = null, NetworkReliability reliability = NetworkReliability.Reliable, INetworkBlockPool? pool = null, CancellationToken cancellation = default)
    {
        sender.SendMessage(connections.OfType<INetworkRemoteConnection>(), pool, security, reliability, message);

        foreach (NetworkLocalConnection connection in connections.OfType<NetworkLocalConnection>())
        {
            await connection.Send(message, security, reliability, pool, cancellation);
        }
    }

    void INetworkChannelProvider.SetUnsynced(INetworkRemoteChannel channel)
        => unsyncedChannels.Add(channel);

    void INetworkChannelProvider.SetSynced(INetworkRemoteChannel channel)
        => unsyncedChannels.Remove(channel);

    private async Task SyncChannels(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            foreach (INetworkRemoteChannel channel in unsyncedChannels)
            {
                channel.TryResend();
            }

            await TimeSpan.FromMilliseconds(50).Delay(cancellation);
        }
    }
}
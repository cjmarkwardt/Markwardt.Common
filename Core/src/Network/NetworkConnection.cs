namespace Markwardt;

public interface INetworkConnection : IDisposable
{
    int Id { get; }
    INetworkHost? Host { get; }
    object? Profile { get; }
    bool IsLocal { get; }
    bool IsOutgoing { get; }
    bool IsSecure { get; }
    bool IsConnected { get; }
    NetworkUser? User { get; }
    IEnumerable<INetworkChannel> Channels { get; }
    IEnumerable<object?> ReceivedChannels { get; }

    bool IsIncoming => !IsOutgoing;

    TimeSpan ChannelIdReuseDelay { get; set; }
    TimeSpan RequestIdReuseDelay { get; set; }

    ValueTask Send(object message, NetworkSecurity? security = null, NetworkReliability reliability = NetworkReliability.Reliable, INetworkBlockPool? pool = null, CancellationToken cancellation = default);
    ValueTask<object> Request(object message, NetworkSecurity? security = null, INetworkBlockPool? pool = null, CancellationToken cancellation = default);
    INetworkChannel OpenChannel(object message, object? profile = null, NetworkSecurity? security = null, INetworkBlockPool? pool = null);
    void Recycle(object message);
    ValueTask Disconnect(string reason, CancellationToken cancellation = default);
}

public abstract class NetworkConnection : BaseDisposable, INetworkConnection
{
    public NetworkConnection(INetworkConnectionProvider provider, INetworkHost? host, object? profile, bool isLocal, bool isOutgoing)
    {
        Id = provider.ConnectionTracker.Add(this);
        Provider = provider;
        Profile = profile;
        Host = host;
        IsLocal = isLocal;
        IsOutgoing = isOutgoing;
        ChannelTracker = new(0) { ReuseDelay = TimeSpan.FromSeconds(30) };
    }

    private readonly Delay delay = new();

    protected INetworkConnectionProvider Provider { get; }
    protected NetworkTracker<INetworkRemoteChannel> ChannelTracker { get; }

    public int Id { get; }

    public object? Profile { get; protected set; }

    public INetworkHost? Host { get; }
    public bool IsLocal { get; }
    public bool IsOutgoing { get; }

    public abstract bool IsSecure { get; }

    public bool IsConnected { get; set; }
    public NetworkUser? User { get; set; }

    public IEnumerable<INetworkChannel> Channels => ChannelTracker.List();

    public abstract IEnumerable<object> ReceivedChannels { get; }

    public TimeSpan ChannelIdReuseDelay { get => ChannelTracker.ReuseDelay; set => ChannelTracker.ReuseDelay = value; }

    public abstract TimeSpan RequestIdReuseDelay { get; set; }

    public INetworkChannel OpenChannel(object message, object? profile = null, NetworkSecurity? security = null, INetworkBlockPool? pool = null)
    {
        NetworkChannel channel = new(ChannelTracker, Provider, message, profile, security, pool);
        channel.Open(this);
        return channel;
    }

    public async ValueTask Send(object message, NetworkSecurity? security = null, NetworkReliability reliability = NetworkReliability.Reliable, INetworkBlockPool? pool = null, CancellationToken cancellation = default)
    {
        await delay.Pass(cancellation);

        if (!IsConnected)
        {
            return;
        }

        ExecuteSend(message, security, reliability, pool);
    }

    public async ValueTask<object> Request(object message, NetworkSecurity? security = null, INetworkBlockPool? pool = null, CancellationToken cancellation = default)
    {
        await delay.Pass(cancellation);

        if (!IsConnected)
        {
            throw new NetworkException("Cannot send a request when disconnected.");
        }

        using CancellationTokenSource linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation, Disposal);
        return await ExecuteRequest(message, security, pool, linkedCancellation.Token);
    }

    public void Recycle(object message)
        => Provider.Handler.OnRecycled(message);

    public async ValueTask Disconnect(string reason, CancellationToken cancellation = default)
    {
        if (IsConnected)
        {
            Release(null);

            await delay.Pass(cancellation);
            SendDisconnect(reason);

            await Task.Delay(500, cancellation);
            Dispose();
        }
    }

    protected abstract void ExecuteSend(object message, NetworkSecurity? security, NetworkReliability reliability, INetworkBlockPool? pool);
    protected abstract ValueTask<object> ExecuteRequest(object message, NetworkSecurity? security, INetworkBlockPool? pool, CancellationToken cancellation = default);
    protected abstract void SendDisconnect(string reason);

    protected void Close(string? reason = null, Exception? innerException = null)
        => Release(new NetworkException(reason ?? "Connection closed", innerException));

    protected override void OnDispose()
    {
        base.OnDispose();
        Release(null);
    }

    private void Release(Exception? exception)
    {
        if (IsConnected)
        {
            IsConnected = false;
            Provider.ConnectionTracker.Remove(Id);
            Provider.Handler.OnDisconnected(this, exception);
        }
    }

    private sealed class Delay
    {
        private bool isPassed;

        public async ValueTask Pass(CancellationToken cancellation = default)
        {
            if (!isPassed)
            {
                await Task.Delay(1, cancellation);
                isPassed = true;
            }
        }
    }
}
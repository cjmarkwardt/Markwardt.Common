namespace Markwardt;

public interface INetworkChannel : IDisposable
{
    object? Profile { get; }
    IEnumerable<INetworkConnection> Connections { get; }

    TimeSpan ResendDelay { get; set; }
    bool SkipDuplicates { get; set; }
    NetworkSecurity? Security { get; set; }
    INetworkBlockPool? Pool { get; set; }

    void Update(object message);
}

public class NetworkChannel : BaseDisposable, INetworkRemoteChannel, INetworkGroupChannel
{
    public NetworkChannel(INetworkTracker<INetworkRemoteChannel> tracker, INetworkChannelProvider provider, object? defaultMessage = null, object? profile = null, NetworkSecurity? security = null, INetworkBlockPool? pool = null)
    {
        this.provider = provider;
        this.tracker = tracker;
        Id = tracker.Add(this);
        this.defaultMessage = defaultMessage;
        this.profile = profile;
        Security = security;
        Pool = pool;
    }

    private readonly INetworkChannelProvider provider;
    private readonly INetworkTracker<INetworkRemoteChannel> tracker;
    private readonly object? defaultMessage;
    private readonly object? profile;
    private readonly HashSet<INetworkRemoteConnection> syncedConnections = [];
    private readonly Dictionary<INetworkLocalConnection, object> localProfiles = [];

    private object? lastUpdate;
    private DateTime lastUpdateTime;
    private byte sequence;

    private IEnumerable<INetworkRemoteConnection> UnsyncedConnections
    {
        get
        {
            foreach (INetworkRemoteConnection connection in remoteConnections)
            {
                if (!syncedConnections.Contains(connection))
                {
                    yield return connection;
                }
            }
        }
    }

    public int Id { get; }
    public object? DefaultMessage => defaultMessage;
    public object? Profile => profile;

    private readonly HashSet<INetworkLocalConnection> localConnections = [];
    private readonly HashSet<INetworkRemoteConnection> remoteConnections = [];
    public IEnumerable<INetworkConnection> Connections => localConnections.Cast<INetworkConnection>().Concat(remoteConnections);

    public TimeSpan ResendDelay { get; set; } = TimeSpan.FromMilliseconds(250);
    public bool SkipDuplicates { get; set; } = true;
    public NetworkSecurity? Security { get; set; }
    public INetworkBlockPool? Pool { get; set; }

    public void Open(INetworkConnection connection, object? message = null)
    {
        message ??= DefaultMessage ?? throw new ArgumentNullException(nameof(message), "Either a message or a default message must be provided to open a group channel.");

        if (connection is INetworkLocalConnection localConnection)
        {
            localConnections.Add(localConnection);
            localProfiles.Add(localConnection, localConnection.ReceiveChannel(message));
        }
        else if (connection is INetworkRemoteConnection remoteConnection)
        {
            remoteConnections.Add(remoteConnection);
            provider.Sender.SendControl(remoteConnection.Yield(), Pool, Security, new NetworkOpenChannelMessage(Id, message));
        }
        else
        {
            throw new ArgumentException("Connection must be either local or remote.", nameof(connection));
        }
    }

    public void Close(INetworkConnection connection)
    {
        if (connection is INetworkLocalConnection localConnection)
        {
            LocalClose(localConnection.Yield());
        }
        else if (connection is INetworkRemoteConnection remoteConnection)
        {
            RemoteClose(remoteConnection.Yield());
        }
        else
        {
            throw new ArgumentException("Connection must be either local or remote.", nameof(connection));
        }
    }

    public void CloseAll()
    {
        LocalClose(localConnections.ToList());
        RemoteClose(remoteConnections);
    }

    public void Update(object message)
    {
        if (SkipDuplicates && message.ValueEquals(lastUpdate))
        {
            return;
        }

        sequence++;
        provider.SetUnsynced(this);
        syncedConnections.Clear();
        RemoteUpdate(message);
        LocalUpdate(message);
    }

    public void RemoteSync(INetworkRemoteConnection connection, byte sequence)
    {
        if (this.sequence == sequence)
        {
            syncedConnections.Add(connection);
            
            if (syncedConnections.IsSupersetOf(remoteConnections))
            {
                provider.SetSynced(this);
            }
        }
    }

    public void TryResend()
    {
        if (DateTime.Now - lastUpdateTime >= ResendDelay && lastUpdate is not null)
        {
            RemoteUpdate(lastUpdate);
        }
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        tracker.Remove(Id);
        CloseAll();
    }

    private void RemoteUpdate(object message)
    {
        lastUpdate = message;
        lastUpdateTime = DateTime.Now;
        provider.Sender.SendUpdate(UnsyncedConnections, Pool, Security, Id, profile, sequence, message);
    }

    private void LocalUpdate(object message)
    {
        foreach (INetworkLocalConnection connection in localConnections)
        {
            if (localProfiles.TryGetValue(connection, out object? profile))
            {
                provider.Handler.OnReceived(connection, profile, message);
            }
        }
    }

    private void RemoteClose(IEnumerable<INetworkRemoteConnection> connections)
    {
        provider.Sender.SendControl(connections, Pool, Security, new NetworkCloseChannelMessage(Id));
        remoteConnections.Clear();
    }

    private void LocalClose(IEnumerable<INetworkLocalConnection> connections)
    {
        foreach (INetworkLocalConnection connection in connections)
        {
            localConnections.Remove(connection);

            if (localProfiles.Remove(connection, out object? profile))
            {
                connection.DestroyReceivedChannel(profile);
            }
        }
    }
}
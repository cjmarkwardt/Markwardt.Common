namespace Markwardt;

public interface INetworkLocalConnection : INetworkConnection
{
    object ReceiveChannel(object message);
    void DestroyReceivedChannel(object channelProfile);
}

public class NetworkLocalConnection(INetworkConnectionProvider provider, object? profile, bool isOutgoing) : NetworkConnection(provider, null, profile, true, isOutgoing), INetworkLocalConnection
{
    private static (NetworkLocalConnection Outgoing, NetworkLocalConnection Incoming) CreatePair(INetworkConnectionProvider provider, object? outgoingProfile, object? incomingProfile)
    {
        NetworkLocalConnection outgoing = new(provider, outgoingProfile, true);
        NetworkLocalConnection incoming = new(provider, incomingProfile, false);
        outgoing.target = incoming;
        incoming.target = outgoing;
        return (outgoing, incoming);
    }

    public static async ValueTask<INetworkConnection> Connect(INetworkConnectionProvider provider, object? request, object? profile, NetworkUser? user, CancellationToken cancellation = default)
    {
        (object? response, object? incomingProfile) = await provider.Handler.OnConnection(null, user, request, true, true, cancellation);
        (NetworkLocalConnection outgoing, NetworkLocalConnection incoming) = CreatePair(provider, profile, incomingProfile);

        outgoing.IsConnected = true;
        incoming.IsConnected = true;
        incoming.User = user;

        provider.Handler.OnConnected(incoming, request);
        provider.Handler.OnConnected(outgoing, response);

        return outgoing;
    }

    private NetworkLocalConnection target = default!;

    public override bool IsSecure => true;

    private readonly HashSet<object> receivedChannels = [];
    public override IEnumerable<object> ReceivedChannels => receivedChannels;

    public override TimeSpan RequestIdReuseDelay { get; set; }

    public object ReceiveChannel(object message)
    {
        object channelProfile = Provider.Handler.OnChannelOpened(this, message).ChannelProfile;
        receivedChannels.Add(channelProfile);
        return channelProfile;
    }

    public void DestroyReceivedChannel(object channelProfile)
    {
        receivedChannels.Remove(channelProfile);
        Provider.Handler.OnChannelClosed(this, channelProfile);
    }

    protected override void ExecuteSend(object message, NetworkSecurity? security, NetworkReliability reliability, INetworkBlockPool? pool)
        => Provider.Handler.OnReceived(target, null, message);

    protected override async ValueTask<object> ExecuteRequest(object message, NetworkSecurity? security, INetworkBlockPool? pool, CancellationToken cancellation = default)
        => (await Provider.Handler.OnRequested(target, message, cancellation)).Response;

    protected override void SendDisconnect(string reason)
        => target.Close(reason);

    protected override void OnDispose()
    {
        base.OnDispose();

        target.Close();
    }
}
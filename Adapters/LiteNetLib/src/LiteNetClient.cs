namespace Markwardt;

public class LiteNetClient(string host, int port, string key) : NetworkClient, INetEventListener
{
    private readonly TaskCompletionSource connect = new();

    private NetManager? manager;
    private NetPeer? peer;
    private bool isConnecting;

    protected override async ValueTask Run(CancellationToken cancellation)
    {
        manager = new(this);
        await manager.Run(cancellation);
    }

    protected override async ValueTask Link(CancellationToken cancellation)
    {
        await base.Link(cancellation);

        manager?.Start();
        isConnecting = true;
        peer = manager?.Connect(host, port, key);
        await connect.Task.WaitAsync(cancellation);
        isConnecting = false;
    }

    protected override ValueTask ExecuteSend(ReadOnlyMemory<byte> data, NetworkConstraints constraints, CancellationToken cancellation)
    {
        peer?.Send(data.Span, LiteNetUtils.GetDeliveryMethod(constraints));
        return ValueTask.CompletedTask;
    }

    protected override async ValueTask Unlink(CancellationToken cancellation)
    {
        await base.Unlink(cancellation);
        peer?.Disconnect();
    }

    protected override void Release()
        => manager?.Stop();

    void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        => request.Reject();

    void INetEventListener.OnPeerConnected(NetPeer peer)
        => connect.SetResult();

    void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        => Receive(reader.RawData.AsSpan(reader.UserDataOffset, reader.UserDataSize));

    void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        NetworkException exception = new(disconnectInfo.Reason.ToString());

        if (isConnecting)
        {
            connect.SetException(exception);
        }
        else
        {
            Drop(exception);
        }
    }

    void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }

    void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency) { }

    void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
}